using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using System.Windows;
using System.Windows.Controls;

namespace CoffeeManagement
{
    public partial class AdminPromotionsView : UserControl
    {
        private PromotionService _promotionService;
        private CoffeeManagementDbContext context;
        private List<Promotion> _allPromotions;

        public AdminPromotionsView()
        {
            InitializeComponent();

            // *** SỬA LỖI Ở ĐÂY ***
            try
            {
                // Bước 1: Khởi tạo DbContext
                context = new CoffeeManagementDbContext();

                // Bước 2: Khởi tạo Service (dùng context đã tạo)
                _promotionService = new PromotionService(new PromotionRepository(new PromotionDAO(context)));

                // Bước 3: Tải dữ liệu (BÂY GIỜ _promotionService đã tồn tại)
                LoadPromotions();
            }
            catch (Exception ex)
            {
                // Báo lỗi nếu không thể khởi tạo service (ví dụ: lỗi kết nối CSDL)
                MessageBox.Show($"Lỗi khởi tạo service: {ex.Message}", "Lỗi nghiêm trọng", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Tải (hoặc tải lại) danh sách khuyến mãi từ service
        /// </summary>
        private void LoadPromotions()
        {
            try
            {
                // *** SỬA LỖI Ở ĐÂY ***
                // Dòng này đã an toàn vì _promotionService đã được tạo trong constructor
                _allPromotions = _promotionService.GetAllPromotion();

                // XÓA DÒNG NÀY ĐI - Nó đã được chuyển lên constructor
                // _promotionService = new PromotionService(...); 

                // Đổ dữ liệu vào ItemsControl
                PromotionsItemsControl.ItemsSource = _allPromotions;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách khuyến mãi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi người dùng gõ vào ô tìm kiếm.
        /// </summary>
        private void TxtSearchPromotion_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Thêm một kiểm tra "null" an toàn
            if (_allPromotions == null) return;

            string searchText = TxtSearchPromotion.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                PromotionsItemsControl.ItemsSource = _allPromotions;
                return;
            }

            var filteredList = _allPromotions.Where(p =>
                p.Code.ToLower().Contains(searchText) ||
                (p.Description != null && p.Description.ToLower().Contains(searchText))
            ).ToList();

            PromotionsItemsControl.ItemsSource = filteredList;
        }

        // ... (Các hàm BtnAdd_Click, BtnEdit_Click, BtnDelete_Click giữ nguyên)
        // Chúng sẽ hoạt động đúng vì _promotionService đã được khởi tạo

        // (Các hàm còn lại của bạn)
        // Trong file: AdminPromotionsView.xaml.cs

        /// <summary>
        /// Mở cửa sổ/dialog để TẠO khuyến mãi mới.
        /// </summary>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // 1. Tạo mới Dialog (không truyền gì vào)
            var dialog = new PromotionEditDialog();

            // 2. Hiển thị Dialog và chờ kết quả
            //    ShowDialog() sẽ tạm dừng code cho đến khi Dialog đóng
            if (dialog.ShowDialog() == true)
            {
                // 3. Nếu người dùng bấm "Lưu" (DialogResult = true)
                //    Lấy đối tượng Promotion đã được điền thông tin từ Dialog
                var newPromotion = dialog.CurrentPromotion;

                try
                {
                    // 4. Gọi Service để tạo mới (BẠN CẦN TẠO HÀM NÀY)
                    _promotionService.AddPromotion(newPromotion);

                    // 5. Tải lại danh sách để thấy mục mới
                    LoadPromotions();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tạo khuyến mãi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Mở cửa sổ/dialog để SỬA khuyến mãi đã chọn.
        /// </summary>
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            // 1. Lấy Promotion cần sửa từ Tag
            var promotionToEdit = button.Tag as Promotion;
            if (promotionToEdit == null) return;

            // 2. Tạo Dialog và TRUYỀN đối tượng vào
            var dialog = new PromotionEditDialog(promotionToEdit);

            // 3. Hiển thị Dialog
            if (dialog.ShowDialog() == true)
            {
                // 4. Nếu người dùng bấm "Lưu", đối tượng 'promotionToEdit'
                //    đã được cập nhật bên trong Dialog.
                //    (Vì C# truyền object theo tham chiếu)

                try
                {
                    // 5. Gọi Service để cập nhật (BẠN CẦN TẠO HÀM NÀY)
                    _promotionService.UpdatePromotion(promotionToEdit);

                    // 6. Tải lại danh sách
                    LoadPromotions();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật khuyến mãi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var promotionToDelete = button.Tag as Promotion;
            if (promotionToDelete == null) return;

            var result = MessageBox.Show($"Bạn có chắc muốn xóa khuyến mãi: {promotionToDelete.Code}?",
                                        "Xác nhận xóa",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Dòng này bây giờ đã an toàn
                    _promotionService.DeletePromotion(promotionToDelete.Id);

                    // Tải lại danh sách để cập nhật giao diện
                    LoadPromotions();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}