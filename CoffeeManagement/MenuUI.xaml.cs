using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using CoffeeManagement.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; // <-- THÊM USING NÀY
using System.Windows.Media.Imaging;

namespace CoffeeManagement
{
    public partial class MenuUI : UserControl
    {
        private readonly MenuItemsService _menuService;
        private readonly OrderService _orderService;
        private readonly UserService _userService; // <-- THÊM USER SERVICE

        private List<DAL.Models.MenuItem> _allMenuItems = new List<DAL.Models.MenuItem>();
        private List<Category> _allCategories = new List<Category>();

        // Biến để lưu khách hàng do nhân viên chọn
        private User _selectedCustomer = null; // <-- THÊM BIẾN NÀY

        public ObservableCollection<CartItem> CartItems { get; set; } = new ObservableCollection<CartItem>();

        private int _selectedCategoryId = 0;
        private string _searchQuery = "";

        public MenuUI()
        {
            InitializeComponent();

            var ctx = new CoffeeManagementDbContext();
            _menuService = new MenuItemsService(new MenuItemsRepository(new MenuItemsDAO(ctx)));
            _orderService = new OrderService(new OrderRepository(new OrderDAO(ctx)));

            // Khởi tạo UserService
            _userService = new UserService(new UserRepository(new UserDao(ctx))); // <-- THÊM DÒNG NÀY

            // Gán DataContext và sources
            this.DataContext = this;
            CartItemsControl.ItemsSource = CartItems;

            CartItems.CollectionChanged += (s, e) =>
            {
                CartItemsControl.Items.Refresh();
                UpdateCartTotal();
            };

            LoadInitialData();

            // Kiểm tra vai trò (Role) sau khi tải dữ liệu
            CheckUserRole(); // <-- THÊM DÒNG NÀY
        }

        // =============================================
        // HÀM MỚI: KIỂM TRA QUYỀN HẠN
        // =============================================
        /// <summary>
        /// Hiển thị/ẩn panel tìm kiếm khách hàng dựa trên vai trò của người đăng nhập
        /// </summary>
        private void CheckUserRole()
        {
            // RoleId 2 = Staff (dựa trên script DB của bạn)
            if (AppSession.CurrentUser != null && AppSession.CurrentUser.RoleId == 2) //
            {
                // Nếu là Staff, hiện panel tìm kiếm
                StaffCustomerSearchPanel.Visibility = Visibility.Visible;
                TxtSelectedCustomerInfo.Text = "Đang chọn: Khách lẻ (Vãng lai)";
                TxtSelectedCustomerInfo.Foreground = Brushes.Gray;
            }
            else
            {
                // Nếu là Customer hoặc không đăng nhập, ẩn panel
                StaffCustomerSearchPanel.Visibility = Visibility.Collapsed;
            }
        }

        // =============================================
        // HÀM MỚI: XỬ LÝ NÚT TÌM KHÁCH HÀNG
        // =============================================
        private void BtnFindCustomer_Click(object sender, RoutedEventArgs e)
        {
            string query = TxtCustomerSearch.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                _selectedCustomer = null;
                TxtSelectedCustomerInfo.Text = "Đang chọn: Khách lẻ (Vãng lai)";
                TxtSelectedCustomerInfo.Foreground = Brushes.Gray;
                return;
            }

            try
            {
                // Tạm thời, chúng ta dùng GetByUsername để kiểm tra
                var foundUser = _userService.GetByUsername(query);

                if (foundUser != null && foundUser.RoleId == 3) // Role 3 là Customer
                {
                    _selectedCustomer = foundUser;
                    TxtSelectedCustomerInfo.Text = $"Đang chọn: {foundUser.FullName} ({foundUser.Username})";
                    TxtSelectedCustomerInfo.Foreground = Brushes.Green;
                }
                else
                {
                    _selectedCustomer = null;
                    TxtSelectedCustomerInfo.Text = "Không tìm thấy khách hàng thành viên.";
                    TxtSelectedCustomerInfo.Foreground = Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                _selectedCustomer = null;
                TxtSelectedCustomerInfo.Text = "Lỗi khi tìm kiếm.";
                TxtSelectedCustomerInfo.Foreground = Brushes.Red;
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }


        private void LoadInitialData()
        {
            try
            {
                _allMenuItems = _menuService.GetAll().ToList();
                _allCategories = _menuService.GetAllCategories().ToList();
                PopulateCategoryFilter();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi nạp dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateCategoryFilter()
        {
            CategoryFilterPanel.Children.Clear();

            var allButton = new RadioButton
            {
                Content = "Tất cả",
                IsChecked = true,
                Style = (Style)FindResource("CategoryButtonStyle"),
                Margin = new Thickness(0, 0, 8, 0),
                Tag = 0
            };
            allButton.Click += CategoryFilter_Click;
            CategoryFilterPanel.Children.Add(allButton);

            foreach (var category in _allCategories)
            {
                var btn = new RadioButton
                {
                    Content = category.Name,
                    Style = (Style)FindResource("CategoryButtonStyle"),
                    Margin = new Thickness(0, 0, 8, 0),
                    Tag = category.Id
                };
                btn.Click += CategoryFilter_Click;
                CategoryFilterPanel.Children.Add(btn);
            }
        }

        private void ApplyFilters()
        {
            var filteredList = _allMenuItems
                .Where(item =>
                    (_selectedCategoryId == 0 || item.CategoryId == _selectedCategoryId) &&
                    (string.IsNullOrEmpty(_searchQuery) || (item.Name?.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0))
                )
                .ToList();

            MenuItemsControl.ItemsSource = filteredList;
        }

        private void CategoryFilter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is int categoryId)
            {
                _selectedCategoryId = categoryId;
                ApplyFilters();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchQuery = TxtSearch.Text ?? "";
            ApplyFilters();
        }

        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DAL.Models.MenuItem? selectedItem = null;
                var fe = sender as FrameworkElement;
                if (fe != null)
                {
                    if (fe.Tag is DAL.Models.MenuItem miTag)
                        selectedItem = miTag;
                    else if (fe.DataContext is DAL.Models.MenuItem miCtx)
                        selectedItem = miCtx;
                }

                if (selectedItem == null) return;

                var existingItem = CartItems.FirstOrDefault(i => i.Item.Id == selectedItem.Id);
                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    CartItems.Add(new CartItem { Item = selectedItem, Quantity = 1 });
                }

                CartItemsControl.Items.Refresh();
                UpdateCartTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm món: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnIncreaseItem_Click(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            CartItem? cartItem = null;
            if (fe != null)
            {
                if (fe.Tag is CartItem tagItem) cartItem = tagItem;
                else if (fe.DataContext is CartItem ctxItem) cartItem = ctxItem;
            }

            if (cartItem == null) return;

            cartItem.Quantity++;
            CartItemsControl.Items.Refresh();
            UpdateCartTotal();
        }

        private void BtnDecreaseItem_Click(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            CartItem? cartItem = null;
            if (fe != null)
            {
                if (fe.Tag is CartItem tagItem) cartItem = tagItem;
                else if (fe.DataContext is CartItem ctxItem) cartItem = ctxItem;
            }

            if (cartItem == null) return;

            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
            }
            else
            {
                CartItems.Remove(cartItem);
            }

            CartItemsControl.Items.Refresh();
            UpdateCartTotal();
        }

        private void UpdateCartTotal()
        {
            decimal total = CartItems.Sum(item => item.Quantity * (item.Item?.Price ?? 0));
            TxtCartTotal.Text = $"{total:N0}đ";
        }


        // =============================================
        // HÀM CHECKOUT ĐÃ ĐƯỢC CẬP NHẬT HOÀN TOÀN
        // =============================================
        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (!CartItems.Any())
            {
                MessageBox.Show("Giỏ hàng trống. Vui lòng chọn món trước khi đặt hàng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- Logic phân quyền Staff/Customer ---
            int? customerIdForOrder = null;
            int? staffIdForOrder = null;
            bool isStaffOrdering = (AppSession.CurrentUser != null && AppSession.CurrentUser.RoleId == 2); //

            if (isStaffOrdering)
            {
                // 1. NGƯỜI DÙNG LÀ NHÂN VIÊN
                staffIdForOrder = AppSession.CurrentUser.Id; // Nhân viên chính là người đang đăng nhập
                customerIdForOrder = _selectedCustomer?.Id; // Khách hàng là người được chọn (hoặc null nếu là khách lẻ)
            }
            else if (AppSession.CurrentUser != null && AppSession.CurrentUser.RoleId == 3 || AppSession.CurrentUser != null && AppSession.CurrentUser.RoleId == 1)
            {
                // 2. NGƯỜI DÙNG LÀ KHÁCH HÀNG (Tự order)
                staffIdForOrder = null;
                customerIdForOrder = AppSession.CurrentUser.Id; // Khách hàng là chính họ
            }
            else
            {
                // 3. KHÁCH LẺ TỰ ORDER (Không đăng nhập)
                MessageBox.Show("Vui lòng đăng nhập để hoàn tất đơn hàng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // --- Kết thúc logic phân quyền ---


            // 1. Tạo đối tượng Order
            var newOrder = new Order
            {
                CustomerId = customerIdForOrder, // Đã gán theo logic phân quyền
                StaffId = staffIdForOrder = AppSession.CurrentUser.Id,     // Đã gán theo logic phân quyền
                CreatedAt = DateTime.Now,
                //PaidAt = DateTime.Now, // Xóa dòng này, vì chưa thanh toán
                Status = 0, // 0 = Pending
                IsPaid = false,
                Note = TxtOrderNote.Text.Trim(),
                TotalAmount = CartItems.Sum(i => i.Quantity * (i.Item?.Price ?? 0)),

                OrderItems = CartItems.Select(ci => new OrderItem
                {
                    MenuItemId = ci.Item.Id,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Item.Price
                }).ToList()
            };

            // 2. Lưu Order vào Database
            try
            {
                _orderService.CreateOrder(newOrder, newOrder.OrderItems.ToList()); //

                // 3. Thông báo và xóa giỏ hàng
                MessageBox.Show($"Đặt hàng thành công! Đơn hàng của bạn đang được xử lý.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                CartItems.Clear();
                TxtOrderNote.Text = string.Empty; // Xóa ghi chú

                // Reset luôn phần tìm kiếm khách hàng (nếu là staff)
                if (isStaffOrdering)
                {
                    _selectedCustomer = null;
                    TxtCustomerSearch.Text = string.Empty;
                    TxtSelectedCustomerInfo.Text = "Đang chọn: Khách lẻ (Vãng lai)";
                    TxtSelectedCustomerInfo.Foreground = Brushes.Gray;
                }

                CartItemsControl.Items.Refresh();
                UpdateCartTotal();
            }
            catch (Exception ex)
            {
                // Hiển thị thông tin lỗi đầy đủ cho debug
                var full = new System.Text.StringBuilder();
                Exception? cur = ex;
                while (cur != null)
                {
                    full.AppendLine(cur.GetType().FullName + ": " + cur.Message);
                    full.AppendLine(cur.StackTrace ?? "");
                    full.AppendLine("----");
                    cur = cur.InnerException;
                }

                System.Diagnostics.Debug.WriteLine("[Order Error] " + full.ToString());
                MessageBox.Show($"Có lỗi xảy ra khi đặt hàng:\n{ex.Message}\n\n(Check Output window for stack trace)", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        // Hàm xử lý ảnh (giữ nguyên)
        private void MenuItemImage_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Image img) return;

            string? imgPath = null;
            if (img.Tag is string tagStr) imgPath = tagStr;
            else if (img.DataContext != null)
            {
                var dc = img.DataContext;
                var prop = dc.GetType().GetProperty("ImgUrl")
                         ?? dc.GetType().GetProperty("ImagePath")
                         ?? dc.GetType().GetProperty("Image")
                         ?? dc.GetType().GetProperty("Url");
                if (prop != null) imgPath = prop.GetValue(dc)?.ToString();
            }

            System.Diagnostics.Debug.WriteLine($"[MenuItemImage_Loaded] raw imgPath='{imgPath}' for DataContext={img.DataContext?.GetType().Name}");

            string asmName = Assembly.GetExecutingAssembly().GetName().Name ?? "CoffeeManagement";
            string fallbackPack = $"pack://application:,,,/{asmName};component/Images/latte.jpg";

            void SetFallback()
            {
                System.Diagnostics.Debug.WriteLine("[MenuItemImage_Loaded] setting fallback image");
                try { img.Source = new BitmapImage(new Uri(fallbackPack, UriKind.Absolute)); }
                catch { img.Source = null; }
            }

            bool TrySetBitmap(Uri uri)
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = uri;
                    bmp.EndInit();
                    bmp.Freeze();
                    img.Source = bmp;
                    System.Diagnostics.Debug.WriteLine($"[MenuItemImage_Loaded] loaded Uri: {uri}");
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MenuItemImage_Loaded] load Uri failed: {uri} -> {ex.Message}");
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(imgPath))
            {
                SetFallback();
                return;
            }

            imgPath = imgPath.Trim();

            if (imgPath.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
            {
                if (TrySetBitmap(new Uri(imgPath, UriKind.Absolute))) return;
            }

            if (imgPath.StartsWith("/")) imgPath = imgPath.TrimStart('/');

            string[] tryExtensions = new[] { "", ".jpg", ".jpeg", ".png", ".gif" };
            string baseName = imgPath;
            string ext = Path.GetExtension(imgPath);
            if (!string.IsNullOrEmpty(ext))
            {
                baseName = imgPath.Substring(0, imgPath.Length - ext.Length);
                tryExtensions = new[] { ext, ".jpg", ".jpeg", ".png", ".gif" };
            }

            if (Uri.TryCreate(imgPath, UriKind.Absolute, out Uri? absoluteUri))
            {
                if (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps || absoluteUri.Scheme == Uri.UriSchemeFile)
                {
                    if (TrySetBitmap(absoluteUri)) return;
                }
            }

            if (Path.IsPathRooted(imgPath))
            {
                if (File.Exists(imgPath))
                {
                    if (TrySetBitmap(new Uri(imgPath, UriKind.Absolute))) return;
                }
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? Directory.GetCurrentDirectory();
            var fileCandidates = new List<string>();
            foreach (var ext2 in tryExtensions.Select(te => baseName + te))
            {
                fileCandidates.Add(Path.Combine(baseDir, ext2));
                fileCandidates.Add(Path.Combine(baseDir, "Images", ext2));
                fileCandidates.Add(Path.Combine(baseDir, "images", ext2));
                fileCandidates.Add(Path.Combine(baseDir, "Resources", ext2));
            }

            foreach (var cand in fileCandidates.Distinct())
            {
                try
                {
                    if (File.Exists(cand))
                    {
                        if (TrySetBitmap(new Uri(cand, UriKind.Absolute))) return;
                    }
                }
                catch { /* ignore */ }
            }

            var packCandidates = new List<string>();
            foreach (var ext2 in tryExtensions.Select(te => baseName + te))
            {
                packCandidates.Add($"pack://application:,,,/{asmName};component/Images/{ext2}");
                packCandidates.Add($"pack://application:,,,/{asmName};component/{ext2}");
                packCandidates.Add($"pack://application:,,,/{asmName};component/images/{ext2}");
                packCandidates.Add($"pack://application:,,,/{asmName};component/{ext2.ToLowerInvariant()}");
                packCandidates.Add($"pack://application:,,,/{asmName};component/Images/{ext2.ToLowerInvariant()}");
            }

            foreach (var p in packCandidates.Distinct())
            {
                try
                {
                    if (TrySetBitmap(new Uri(p, UriKind.Absolute))) return;
                }
                catch { }
            }

            SetFallback();
        }
    }
}