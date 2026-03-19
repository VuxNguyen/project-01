using System;
using System.Windows;
using WarehouseManagement.Models;

namespace WarehouseManagement
{
    public partial class EditWindow : Window
    {
        private readonly Helmet _originalHelmet;
        public Helmet? UpdatedHelmet { get; private set; }

        public EditWindow(Helmet helmet)
        {
            InitializeComponent();
            _originalHelmet = helmet;
            LoadHelmetData();
        }

        private void LoadHelmetData()
        {
            txtId.Text = _originalHelmet.Id;
            txtHelmetType.Text = _originalHelmet.HelmetType;
            txtSize.Text = _originalHelmet.Size;
            txtBrand.Text = _originalHelmet.Brand;
            dtpImportDate.SelectedDate = _originalHelmet.ImportDate != DateTime.MinValue ? _originalHelmet.ImportDate : DateTime.Now;
            txtMaterial.Text = _originalHelmet.Material;
            txtPrice.Text = _originalHelmet.Price.ToString();
            txtColor.Text = _originalHelmet.Color;
            txtWeight.Text = _originalHelmet.Weight.ToString();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            UpdatedHelmet = new Helmet
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
