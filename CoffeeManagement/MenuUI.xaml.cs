using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoffeeManagement
{
    public partial class MenuUI : UserControl
    {
        private List<DAL.Models.MenuItem> _menuItems = new();
        private List<CartItem> _cart = new(); // Giỏ hàng tạm

        public MenuUI()
        {
            InitializeComponent();
            LoadMenuItems();
        }

        /// <summary>
        /// Lấy danh sách món ăn từ DB qua tầng BLL
        /// </summary>
        private void LoadMenuItems()
        {
            try
            {
                using var ctx = new CoffeeManagementDbContext();
                var dao = new MenuItemsDAO(ctx);
                var repo = new MenuItemsRepository(dao);
                var service = new MenuItemsService(repo);

                _menuItems = service.GetAll().Where(m => m.IsAvailable).ToList();
                RenderMenuCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải menu: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Hiển thị danh sách món dạng thẻ (card)
        /// </summary>
        private void RenderMenuCards()
        {
            WrapMenuItems.Children.Clear();

            foreach (var item in _menuItems)
            {
                Border card = CreateMenuCard(item);
                WrapMenuItems.Children.Add(card);
            }
        }

        /// <summary>
        /// Tạo card UI cho mỗi món ăn
        /// </summary>
        private Border CreateMenuCard(DAL.Models.MenuItem item)
        {
            // Border tổng thể
            Border card = new Border
            {
                Width = 280,
                Height = 330,
                Margin = new Thickness(10),
                CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210)),
                Background = Brushes.White,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 4,
                    ShadowDepth = 2,
                    Opacity = 0.25
                }
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(8) };

            // Hình ảnh
            Image img = new Image
            {
                Width = 250,
                Height = 200,
                Stretch = Stretch.UniformToFill,
                Margin = new Thickness(0, 0, 0, 5)
            };

            if (!string.IsNullOrEmpty(item.ImgUrl))
            {
                try
                {
                    string imagePath = $"pack://application:,,,{item.ImgUrl}";
                    img.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                }
                catch
                {
                    img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/latte.jpg", UriKind.Absolute));
                }

            }

            // Tên món
            TextBlock name = new TextBlock
            {
                Text = item.Name,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 2)
            };

            // Mô tả món
            TextBlock desciption = new TextBlock
            {
                Text = item.Description,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 2)
            };

            // Giá
            TextBlock price = new TextBlock
            {
                Text = $"{item.Price:C0}",
                Foreground = new SolidColorBrush(Color.FromRgb(50, 150, 50)),
                FontSize = 13,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 2, 0, 5)
            };

            // Nút thêm vào giỏ hàng
            Button addBtn = new Button
            {
                Content = "Add to cart",
                Background = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
                Foreground = Brushes.White,
                Padding = new Thickness(5),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            addBtn.Click += (s, e) => AddToCart(item);

            panel.Children.Add(img);
            panel.Children.Add(name);
            panel.Children.Add(desciption);
            panel.Children.Add(price);
            panel.Children.Add(addBtn);

            card.Child = panel;
            return card;
        }

        /// <summary>
        /// Thêm món vào giỏ hàng
        /// </summary>
        private void AddToCart(DAL.Models.MenuItem item)
        {
            // Tìm xem món này đã có trong giỏ chưa
            var existing = _cart.FirstOrDefault(c => c.Item.Id == item.Id);
            if (existing != null)
            {
                existing.Quantity++; // nếu có, tăng số lượng
            }
            else
            {
                _cart.Add(new CartItem { Item = item, Quantity = 1 });
            }

            UpdateCartBadge();
            MessageBox.Show($"{item.Name} đã được thêm vào giỏ hàng.",
                            "Cart", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateCartBadge()
        {
            int totalQty = _cart.Sum(c => c.Quantity);

            if (totalQty > 0)
            {
                CartBadge.Visibility = Visibility.Visible;
                TxtCartCount.Text = totalQty.ToString();
            }
            else
            {
                CartBadge.Visibility = Visibility.Collapsed;
            }

        }

        private void BtnCart_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CartDialog(_cart);
            bool? result = dlg.ShowDialog();
          

            // Sau khi đóng dialog, cập nhật lại (phòng khi user xoá món trong giỏ)
            UpdateCartBadge();
        }
    }
}
