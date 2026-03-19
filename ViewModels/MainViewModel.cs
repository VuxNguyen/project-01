using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WarehouseManagement.Models;

namespace WarehouseManagement.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Helmet> _helmets = new ObservableCollection<Helmet>();

        public ObservableCollection<Helmet> Helmets
        {
            get => _helmets;
            set
            {
                _helmets = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalHelmets));
            }
        }

        public int TotalHelmets => Helmets.Count;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddHelmet(Helmet helmet)
        {
            Helmets.Add(helmet);
            OnPropertyChanged(nameof(TotalHelmets));
        }

        public void RemoveHelmet(Helmet helmet)
        {
            Helmets.Remove(helmet);
            OnPropertyChanged(nameof(TotalHelmets));
        }

        public void UpdateHelmet(Helmet oldHelmet, Helmet newHelmet)
        {
            int index = Helmets.IndexOf(oldHelmet);
            if (index >= 0)
            {
                Helmets[index] = newHelmet;
            }
        }
    }
}
