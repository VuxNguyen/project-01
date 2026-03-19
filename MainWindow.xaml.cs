using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WarehouseManagement.Models;
using WarehouseManagement.ViewModels;

namespace WarehouseManagement
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private List<Helmet> _allHelmets = new List<Helmet>();

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            dtpImportDate.SelectedDate = DateTime.Now;
            UpdateTotalHelmets();
        }

        private void UpdateTotalHelmets()
        {
            lblTotalHelmets.Text = _viewModel.TotalHelmets.ToString();
            dgvHelmets.ItemsSource = _allHelmets;
        }

        private void FilterHelmets()
        {
            var filtered = string.IsNullOrEmpty(txtSearch.Text)
                ? _viewModel.Helmets.ToList()
                : _viewModel.Helmets.Where(h =>
                    h.HelmetType.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                    h.Id.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                    h.Brand.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                    h.Color.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)).ToList();

            _allHelmets = filtered;
            dgvHelmets.ItemsSource = null;
            dgvHelmets.ItemsSource = _allHelmets;
            UpdateTotalHelmets();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterHelmets();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            FilterHelmets();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text) || string.IsNullOrEmpty(txtHelmetType.Text))
            {
                MessageBox.Show("Vui lòng nhập mã nón và loại nón!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existing = _viewModel.Helmets.FirstOrDefault(h => h.Id == txtId.Text);
            if (existing != null)
            {
                MessageBox.Show("Mã nón đã tồn tại!", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var helmet = new Helmet
            {
                Id = txtId.Text,
                HelmetType = txtHelmetType.Text,
                Size = txtSize.Text,
                Brand = txtBrand.Text,
                ImportDate = dtpImportDate.SelectedDate ?? DateTime.Now,
                Material = txtMaterial.Text,
                Price = decimal.TryParse(txtPrice.Text, out decimal p) ? p : 0,
                Color = txtColor.Text,
                Weight = double.TryParse(txtWeight.Text, out double w) ? w : 0
            };

            _viewModel.AddHelmet(helmet);
            FilterHelmets();
            ClearForm();
            MessageBox.Show("✅ Đã thêm nón mới thành công!", "Thành công", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearForm()
        {
            txtId.Text = "";
            txtHelmetType.Text = "";
            txtSize.Text = "";
            txtBrand.Text = "";
            dtpImportDate.SelectedDate = DateTime.Now;
            txtMaterial.Text = "";
            txtPrice.Text = "";
            txtColor.Text = "";
            txtWeight.Text = "";
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgvHelmets.SelectedItem is Helmet helmet)
            {
                var editWindow = new EditWindow(helmet);
                if (editWindow.ShowDialog() == true && editWindow.UpdatedHelmet != null)
                {
                    var updatedHelmet = editWindow.UpdatedHelmet;
                    _viewModel.UpdateHelmet(helmet, updatedHelmet);
                    FilterHelmets();
                    UpdateTotalHelmets();
                    MessageBox.Show("✅ Đã cập nhật thông tin nón!", "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn nón cần chỉnh sửa!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgvHelmets.SelectedItem is Helmet helmet)
            {
                var confirmWindow = new ConfirmDeleteWindow(helmet);
                if (confirmWindow.ShowDialog() == true)
                {
                    _viewModel.RemoveHelmet(helmet);
                    FilterHelmets();
                    UpdateTotalHelmets();
                    MessageBox.Show("✅ Đã xóa nón khỏi kho!", "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn nón cần xóa!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
