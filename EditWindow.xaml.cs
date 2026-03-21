using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            SetupValidation();
        }

        private void LoadHelmetData()
        {
            txtId.Text = _originalHelmet.Id;
            
            // Set selected value for HelmetType ComboBox
            cmbHelmetType.SelectedItem = null;
            foreach (var item in cmbHelmetType.Items)
            {
                if (item is ComboBoxItem comboItem && 
                    comboItem.Content?.ToString() == _originalHelmet.HelmetType)
                {
                    cmbHelmetType.SelectedItem = item;
                    break;
                }
            }
            
            // Set selected value for Size ComboBox
            cmbSize.SelectedItem = null;
            foreach (var item in cmbSize.Items)
            {
                if (item is ComboBoxItem comboItem && 
                    comboItem.Content?.ToString() == _originalHelmet.Size)
                {
                    cmbSize.SelectedItem = item;
                    break;
                }
            }
            
            // Set selected value for Brand ComboBox
            cmbBrand.SelectedItem = null;
            foreach (var item in cmbBrand.Items)
            {
                if (item is ComboBoxItem comboItem && 
                    comboItem.Content?.ToString() == _originalHelmet.Brand)
                {
                    cmbBrand.SelectedItem = item;
                    break;
                }
            }
            
            dtpImportDate.SelectedDate = _originalHelmet.ImportDate != DateTime.MinValue ? _originalHelmet.ImportDate : DateTime.Now;
            txtMaterial.Text = _originalHelmet.Material;
            txtPrice.Text = _originalHelmet.Price.ToString("N0");
            txtColor.Text = _originalHelmet.Color;
            txtWeight.Text = _originalHelmet.Weight.ToString("F0");
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
            e.Handled = !IsTextAllowed(e.Text, allowOnlyDigits: true);
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
                if (!IsTextAllowed(text, allowOnlyDigits: true))
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
            e.Handled = !IsTextAllowed(e.Text, allowOnlyDigits: true);
        }

        private void TxtPrice_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text, allowOnlyDigits: true))
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

        private bool IsTextAllowed(string text, bool allowOnlyDigits)
        {
            if (allowOnlyDigits)
            {
                // Only allow digits 0-9
                return Regex.IsMatch(text, @"^[0-9]+$");
            }
            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Get selected values from ComboBoxes
            string helmetType = (cmbHelmetType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string size = (cmbSize.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string brand = (cmbBrand.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

            // Parse price - remove thousand separators
            decimal price = 0;
            string priceText = txtPrice.Text.Replace(".", "").Replace(",", "");
            decimal.TryParse(priceText, out price);

            // Parse weight (integer grams)
            int.TryParse(txtWeight.Text, out int weight);

            UpdatedHelmet = new Helmet
            {
                Id = txtId.Text,
                HelmetType = helmetType,
                Size = size,
                Brand = brand,
                ImportDate = dtpImportDate.SelectedDate ?? DateTime.Now,
                Material = txtMaterial.Text,
                Price = price,
                Color = txtColor.Text,
                Weight = weight
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
