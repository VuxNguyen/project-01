using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using WarehouseManagement.Models;

namespace WarehouseManagement.Services
{
    public static class BrandManager
    {
        private static List<Brand> _brands = new List<Brand>();
        private static readonly string LogoFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LOGO");

        public static List<Brand> Brands => _brands;

        static BrandManager()
        {
            LoadBrandsFromLogoFolder();
        }

        private static void LoadBrandsFromLogoFolder()
        {
            _brands.Clear();

            // Add default brands from the existing LOGO folder
            if (Directory.Exists(LogoFolder))
            {
                string[] logoFiles = Directory.GetFiles(LogoFolder);
                foreach (string file in logoFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string extension = Path.GetExtension(file).ToLower();

                    // Only process image files
                    if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
                    {
                        _brands.Add(new Brand
                        {
                            Name = fileName,
                            LogoPath = file
                        });
                    }
                }
            }
            else
            {
                // Create default brands if LOGO folder doesn't exist
                _brands = new List<Brand>
                {
                    new Brand { Name = "NÓN ANDES", LogoPath = "" },
                    new Brand { Name = "NÓN ASIA", LogoPath = "" },
                    new Brand { Name = "NÓN SƠN", LogoPath = "" },
                    new Brand { Name = "NÓN FALCON", LogoPath = "" },
                    new Brand { Name = "NÓN KYT", LogoPath = "" },
                    new Brand { Name = "NÓN NITRINOS", LogoPath = "" },
                    new Brand { Name = "NÓN NONA", LogoPath = "" },
                    new Brand { Name = "NÓN POC", LogoPath = "" },
                    new Brand { Name = "NÓN ROC", LogoPath = "" },
                    new Brand { Name = "NÓN ROYAL", LogoPath = "" },
                    new Brand { Name = "NÓN SUNDA", LogoPath = "" },
                    new Brand { Name = "NÓN ZEUS", LogoPath = "" }
                };
            }
        }

        public static void RefreshBrands()
        {
            LoadBrandsFromLogoFolder();
        }

        public static bool AddBrand(string brandName, string logoPath)
        {
            if (string.IsNullOrWhiteSpace(brandName))
            {
                return false;
            }

            // Check if brand already exists
            if (_brands.Any(b => b.Name.Equals(brandName, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            // Ensure LOGO folder exists
            if (!Directory.Exists(LogoFolder))
            {
                Directory.CreateDirectory(LogoFolder);
            }

            // Copy logo file to LOGO folder if provided
            string newLogoPath = "";
            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                string extension = Path.GetExtension(logoPath);
                string newFileName = brandName.Replace(" ", "_") + extension;
                newLogoPath = Path.Combine(LogoFolder, newFileName);

                try
                {
                    File.Copy(logoPath, newLogoPath, true);
                }
                catch
                {
                    newLogoPath = "";
                }
            }

            // Add new brand
            _brands.Add(new Brand
            {
                Name = brandName,
                LogoPath = newLogoPath
            });

            return true;
        }

        public static bool RemoveBrand(string brandName)
        {
            var brand = _brands.FirstOrDefault(b => b.Name.Equals(brandName, StringComparison.OrdinalIgnoreCase));
            if (brand != null)
            {
                // Don't delete the logo file, just remove from the list
                _brands.Remove(brand);
                return true;
            }
            return false;
        }

        public static BitmapImage? GetBrandLogo(string brandName)
        {
            var brand = _brands.FirstOrDefault(b => b.Name.Equals(brandName, StringComparison.OrdinalIgnoreCase));
            if (brand != null && !string.IsNullOrEmpty(brand.LogoPath) && File.Exists(brand.LogoPath))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(brand.LogoPath, UriKind.Absolute);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public static List<string> GetBrandNames()
        {
            return _brands.Select(b => b.Name).OrderBy(n => n).ToList();
        }
    }
}
