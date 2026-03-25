using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WarehouseManagement.Models;
using WarehouseManagement.Services;

namespace WarehouseManagement
{
    public partial class BrandFilterWindow : Window
    {
        private HashSet<string> _selectedBrands = new HashSet<string>();
        private List<Brand> _allBrands;

        public List<string> SelectedBrands { get; private set; } = new List<string>();

        public BrandFilterWindow(List<string>? currentlySelectedBrands = null)
        {
            InitializeComponent();
            
            // Refresh brands from LOGO folder
            BrandManager.RefreshBrands();
            _allBrands = BrandManager.Brands;
            
            // Load brands into ItemsControl
            BrandList.ItemsSource = _allBrands;
            
            // Set initial selection
            if (currentlySelectedBrands != null)
            {
                foreach (var brand in currentlySelectedBrands)
                {
                    _selectedBrands.Add(brand);
                }
            }
            
            UpdateBrandListDisplay();
        }

        private void UpdateBrandListDisplay()
        {
            // Force refresh the ItemsControl
            BrandList.ItemsSource = null;
            BrandList.ItemsSource = _allBrands.Select(b => new BrandItemViewModel
            {
                Name = b.Name,
                LogoPath = GetLogoImage(b.LogoPath),
                IsSelected = _selectedBrands.Contains(b.Name),
                CheckIcon = _selectedBrands.Contains(b.Name) ? "✓" : "",
                SelectionBackground = _selectedBrands.Contains(b.Name) ? "#e8f8f5" : "White",
                SelectionBorderColor = _selectedBrands.Contains(b.Name) ? "#27ae60" : "#ecf0f1"
            }).ToList();
        }

        private ImageSource? GetLogoImage(string logoPath)
        {
            if (string.IsNullOrEmpty(logoPath) || !System.IO.File.Exists(logoPath))
            {
                return null;
            }

            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(logoPath, UriKind.Absolute);
                bitmap.DecodePixelWidth = 40;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private void BrandItem_Click(object sender, RoutedEventArgs e)
        {
            // Handle click on both Button and Border elements
            string? brandName = null;
            
            if (sender is System.Windows.Controls.Button button)
            {
                brandName = button.Tag as string;
            }
            else if (sender is System.Windows.Controls.Border border)
            {
                brandName = border.Tag as string;
            }
            
            if (!string.IsNullOrEmpty(brandName))
            {
                if (_selectedBrands.Contains(brandName))
                {
                    _selectedBrands.Remove(brandName);
                }
                else
                {
                    _selectedBrands.Add(brandName);
                }
                
                UpdateBrandListDisplay();
            }
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            _selectedBrands.Clear();
            UpdateBrandListDisplay();
        }

        private void BtnAddBrand_Click(object sender, RoutedEventArgs e)
        {
            var addBrandWindow = new AddBrandWindow();
            addBrandWindow.Owner = this;
            if (addBrandWindow.ShowDialog() == true)
            {
                // Refresh brands list
                BrandManager.RefreshBrands();
                _allBrands = BrandManager.Brands;
                UpdateBrandListDisplay();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            SelectedBrands = _selectedBrands.ToList();
            DialogResult = true;
            Close();
        }
    }

    public class BrandItemViewModel
    {
        public string Name { get; set; } = string.Empty;
        public ImageSource? LogoPath { get; set; }
        public bool IsSelected { get; set; }
        public string CheckIcon { get; set; } = string.Empty;
        public string SelectionBackground { get; set; } = "White";
        public string SelectionBorderColor { get; set; } = "#ecf0f1";
    }
}
