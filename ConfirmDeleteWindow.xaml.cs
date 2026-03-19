using System;
using System.Windows;
using WarehouseManagement.Models;

namespace WarehouseManagement
{
    public partial class ConfirmDeleteWindow : Window
    {
        private readonly Helmet _helmet;

        public ConfirmDeleteWindow(Helmet helmet)
        {
            InitializeComponent();
            _helmet = helmet;
            LoadHelmetInfo();
        }

        private void LoadHelmetInfo()
        {
            lblId.Text = _helmet.Id;
            lblHelmetType.Text = _helmet.HelmetType;
            lblSize.Text = _helmet.Size;
            lblBrand.Text = _helmet.Brand;
            lblImportDate.Text = _helmet.ImportDate != DateTime.MinValue
                ? _helmet.ImportDate.ToString("dd/MM/yyyy")
                : "N/A";
            lblMaterial.Text = _helmet.Material;
            lblPrice.Text = _helmet.Price.ToString("N0") + " ₫";
            lblColor.Text = _helmet.Color;
            lblWeight.Text = _helmet.Weight.ToString("F0") + " gram";
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
