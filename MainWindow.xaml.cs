using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WarehouseManagement.Models;
using WarehouseManagement.Services;
using WarehouseManagement.ViewModels;

namespace WarehouseManagement
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private List<Helmet> _allHelmets = new List<Helmet>();
        private List<string> _selectedBrands = new List<string>();
        private double _baseWidth = 1200;
        private double _baseHeight = 700;
        private bool _isInitialized = false;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            dtpImportDate.SelectedDate = DateTime.Now;
            SetupValidation();
            LoadBrandsIntoComboBox();
            UpdateTotalHelmets();
        }

        private void LoadBrandsIntoComboBox()
        {
            // Refresh brands from LOGO folder
            BrandManager.RefreshBrands();
            
            // Clear existing items
            cmbBrand.Items.Clear();
            
            // Add brands from BrandManager (without logo display)
            foreach (var brandName in BrandManager.GetBrandNames())
            {
                cmbBrand.Items.Add(new ComboBoxItem 
                { 
                    Content = brandName,
                    Tag = brandName
                });
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isInitialized = true;
            ApplyResponsiveLayout();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isInitialized)
            {
                ApplyResponsiveLayout();
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (_isInitialized)
            {
                ApplyResponsiveLayout();
            }
        }

        private void ApplyResponsiveLayout()
        {
            double currentWidth = ActualWidth;
            double currentHeight = ActualHeight;
            
            // Calculate scale factor based on window size
            double scaleX = Math.Max(0.7, Math.Min(1.5, currentWidth / _baseWidth));
            double scaleY = Math.Max(0.7, Math.Min(1.5, currentHeight / _baseHeight));
            double scale = Math.Min(scaleX, scaleY);

            // Adjust header font sizes using FindName
            var headerTitle = FindName("HeaderTitle") as TextBlock;
            var headerSubtitle = FindName("HeaderSubtitle") as TextBlock;
            var headerIcon = FindName("HeaderIcon") as TextBlock;
            
            if (headerTitle != null)
            {
                headerTitle.FontSize = Math.Max(16, 24 * scale);
            }
            if (headerSubtitle != null)
            {
                headerSubtitle.FontSize = Math.Max(10, 12 * scale);
            }
            if (headerIcon != null)
            {
                headerIcon.FontSize = Math.Max(20, 28 * scale);
            }

            // Adjust total count section
            var totalCountLabel = FindName("TotalCountLabel") as TextBlock;
            var totalCountIcon = FindName("TotalCountIcon") as TextBlock;
            
            if (totalCountLabel != null)
            {
                totalCountLabel.FontSize = Math.Max(11, 14 * scale);
            }
            if (lblTotalHelmets != null)
            {
                lblTotalHelmets.FontSize = Math.Max(22, 32 * scale);
            }
            if (totalCountIcon != null)
            {
                totalCountIcon.FontSize = Math.Max(18, 24 * scale);
            }

            // Adjust TabControl margins based on window size
            var mainTabControl = FindName("MainTabControl") as TabControl;
            if (mainTabControl != null)
            {
                double margin = Math.Max(10, 15 * scale);
                mainTabControl.Margin = new Thickness(margin);
            }

            // Adjust button sizes proportionally
            AdjustButtonSizes(scale);
        }

        private void AdjustButtonSizes(double scale)
        {
            // Button MinWidth scaling using FindName
            var btnEdit = FindName("btnEdit") as Button;
            var btnDelete = FindName("btnDelete") as Button;
            
            if (btnEdit != null)
            {
                btnEdit.MinWidth = Math.Max(80, 100 * scale);
            }
            if (btnDelete != null)
            {
                btnDelete.MinWidth = Math.Max(60, 80 * scale);
            }
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

        private bool _isUpdatingPrice = false;

        private void TxtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingPrice) return;

            // Get current cursor position
            int caretIndex = txtPrice.CaretIndex;

            // Get raw digits only (remove dots and commas)
            string rawText = txtPrice.Text.Replace(".", "").Replace(",", "");

            // Check if user is deleting
            bool isDeleting = rawText.Length < _previousPriceLength;
            _previousPriceLength = rawText.Length;

            // Format with thousand separators
            if (!string.IsNullOrEmpty(rawText))
            {
                if (long.TryParse(rawText, out long value) && value > 0)
                {
                    string formatted = value.ToString("N0", new CultureInfo("vi-VN"));

                    // Adjust for dots that may have been added/removed
                    if (!isDeleting)
                    {
                        // Adding digits - position cursor after the last digit
                        caretIndex = formatted.Length;
                    }
                    else
                    {
                        // Deleting - maintain relative position
                        caretIndex = Math.Min(caretIndex, formatted.Length);
                    }

                    _isUpdatingPrice = true;
                    txtPrice.Text = formatted;
                    txtPrice.CaretIndex = Math.Min(caretIndex, formatted.Length);
                    _isUpdatingPrice = false;
                }
            }
        }

        private int _previousPriceLength = 0;

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

            // Apply brand filter if any brands are selected
            if (_selectedBrands.Count > 0)
            {
                filtered = filtered.Where(h => 
                    _selectedBrands.Any(b => h.Brand.Equals(b, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            _allHelmets = filtered;
            dgvHelmets.ItemsSource = null;
            dgvHelmets.ItemsSource = _allHelmets;
            UpdateTotalHelmets();
        }

        private void BtnBrandFilter_Click(object sender, RoutedEventArgs e)
        {
            var brandFilterWindow = new BrandFilterWindow(_selectedBrands);
            brandFilterWindow.Owner = this;
            
            if (brandFilterWindow.ShowDialog() == true)
            {
                _selectedBrands = brandFilterWindow.SelectedBrands;
                
                // Update button text to show filter status using FindName
                var button = FindName("btnBrandFilter") as Button;
                if (button != null)
                {
                    if (_selectedBrands.Count > 0)
                    {
                        button.Content = $"🏷️ Lọc hãng ({_selectedBrands.Count})";
                        button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8e44ad"));
                    }
                    else
                    {
                        button.Content = "🏷️ Lọc hãng";
                        button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9b59b6"));
                    }
                }
                
                FilterHelmets();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterHelmets();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            _selectedBrands.Clear();
            
            // Reset brand filter button
            var button = FindName("btnBrandFilter") as Button;
            if (button != null)
            {
                button.Content = "🏷️ Lọc hãng";
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9b59b6"));
            }
            
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

        private void BtnAddBrand_Click(object sender, RoutedEventArgs e)
        {
            var addBrandWindow = new AddBrandWindow();
            addBrandWindow.Owner = this;
            
            if (addBrandWindow.ShowDialog() == true)
            {
                // Refresh brands list
                LoadBrandsIntoComboBox();
                MessageBox.Show("✅ Đã thêm hãng sản xuất mới!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
