using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using WarehouseManagement.Services;

namespace WarehouseManagement
{
    public partial class AddBrandWindow : Window
    {
        private string _selectedLogoPath = "";

        public AddBrandWindow()
        {
            InitializeComponent();
        }

        private void BtnBrowseLogo_Click(object sender, RoutedEventArgs e)
        {
            // Open file dialog to select logo - support all common image formats
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Chọn logo hãng sản xuất",
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.gif|PNG Files|*.png|JPEG Files|*.jpg;*.jpeg|GIF Files|*.gif",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedLogoPath = openFileDialog.FileName;
                string fileName = Path.GetFileName(_selectedLogoPath);
                txtLogoPath.Text = fileName;
                
                // Display preview
                DisplayImagePreview(_selectedLogoPath);
            }
        }

        private void DisplayImagePreview(string imagePath)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.DecodePixelWidth = 200;
                bitmap.EndInit();
                bitmap.Freeze();
                
                LogoPreview.Source = bitmap;
                
                // Update preview border to show image is loaded
                LogoPreviewBorder.Background = new SolidColorBrush(Color.FromRgb(236, 240, 241));
            }
            catch
            {
                MessageBox.Show("Không thể tải hình ảnh. Vui lòng chọn file khác!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                _selectedLogoPath = "";
                txtLogoPath.Text = "Chưa chọn logo";
                LogoPreview.Source = null;
                LogoPreviewBorder.Background = new SolidColorBrush(Color.FromRgb(236, 240, 241));
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate brand name
            string brandName = txtBrandName.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(brandName))
            {
                MessageBox.Show("Vui lòng nhập tên hãng sản xuất!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtBrandName.Focus();
                return;
            }

            // Add "NÓN" prefix if not already present
            if (!brandName.ToUpper().StartsWith("NÓN"))
            {
                brandName = "NÓN " + brandName;
            }
            
            // Add brand
            bool success = BrandManager.AddBrand(brandName, _selectedLogoPath);
            
            if (success)
            {
                MessageBox.Show($"✅ Đã thêm hãng '{brandName}' thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Hãng sản xuất này đã tồn tại!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
