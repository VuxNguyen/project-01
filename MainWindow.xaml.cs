using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            SetupValidation();
            UpdateTotalHelmets();
        }

        private void SetupValidation()
        {
            // Weight - only positive integers (grams)
            txtWeight.PreviewTextInput += TxtWeight_PreviewTextInput;
            txtWeight.TextChanged += TxtWeight_TextChanged;
            DataObject.AddPastingHandler(txtWeight, TxtWeight_Pasting);

            // Price - only positive integers with thousand separator
            txtPrice.TextChanged += TxtPrice_TextChanged;
            txtPrice.PreviewKeyDown += TxtPrice_PreviewKeyDown;
            txtPrice.PreviewTextInput += TxtPrice_PreviewTextInput;
            DataObject.AddPastingHandler(txtPrice, TxtPrice_Pasting);
        }

        // Weight validation - only digits
        private void TxtWeight_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void TxtWeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Remove any non-digit characters
            string text = txtWeight.Text;
            string digitsOnly = new string(text.Where(char.IsDigit).ToArray());
            
            if (text != digitsOnly)
            {
                txtWeight.Text = digitsOnly;
                txtWeight.CaretIndex = digitsOnly.Length;
            }
        }

        private void TxtWeight_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        // Price validation - only digits
        private void TxtPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void TxtPrice_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        public void TxtPrice_LostFocus(object sender, RoutedEventArgs e)
        {
            FormatPriceDisplay();
        }

        private void TxtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            // First, remove any non-digit characters
            string text = txtPrice.Text;
            string digitsOnly = new string(text.Where(char.IsDigit).ToArray());
            
            if (text != digitsOnly)
            {
                txtPrice.Text = digitsOnly;
                txtPrice.CaretIndex = digitsOnly.Length;
                return;
            }
            
            // Then format with thousand separators
            FormatPriceDisplay();
        }

        private void TxtPrice_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Move to next control or complete input
                txtPrice.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }

        private void FormatPriceDisplay()
        {
            // Get raw digits only (remove dots and commas)
            string rawText = txtPrice.Text.Replace(".", "").Replace(",", "");
            
            if (long.TryParse(rawText, out long value) && value > 0)
            {
                // Use Vietnamese culture with dot as thousand separator
                string formatted = value.ToString("N0", new CultureInfo("vi-VN"));
                
                // Only update if different to avoid flickering
                if (txtPrice.Text != formatted)
                {
                    txtPrice.Text = formatted;
                    txtPrice.CaretIndex = formatted.Length;
                }
            }
            else if (string.IsNullOrWhiteSpace(txtPrice.Text) || txtPrice.Text == "0")
            {
                // Keep empty or zero as is
            }
        }

        private bool IsTextAllowed(string text)
        {
            // Only allow digits 0-9
            return Regex.IsMatch(text, @"^[0-9]+$");
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
            if (string.IsNullOrEmpty(txtId.Text))
            {
                MessageBox.Show("Vui lòng nhập mã nón!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get selected values from ComboBoxes
            string helmetType = (cmbHelmetType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string size = (cmbSize.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string brand = (cmbBrand.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

            if (string.IsNullOrEmpty(helmetType))
            {
                MessageBox.Show("Vui lòng chọn loại nón!", "Thông báo",
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

            // Parse price - remove thousand separators
            decimal price = 0;
            if (txtPrice.Text.Contains("."))
            {
                decimal.TryParse(txtPrice.Text.Replace(".", ""), out price);
            }
            else
            {
                decimal.TryParse(txtPrice.Text, out price);
            }

            // Parse weight (integer grams)
            int.TryParse(txtWeight.Text, out int weight);

            var helmet = new Helmet
            {
                Id = txtId.Text,
                HelmetType = helmetType,
                Size = size ?? "",
                Brand = brand ?? "",
                ImportDate = dtpImportDate.SelectedDate ?? DateTime.Now,
                Material = txtMaterial.Text,
                Price = price,
                Color = txtColor.Text,
                Weight = weight
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
            cmbHelmetType.SelectedItem = null;
            cmbSize.SelectedItem = null;
            cmbBrand.SelectedItem = null;
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
