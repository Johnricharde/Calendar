using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Calendar
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DateTime _currentDate;
        private string _currentDayOfWeek;
        private int _currentDay;
        private int _currentMonth;
        private int _currentYear;

        public string FormattedMonthAndYear => $"{CurrentDate.ToString("MMMM yyyy")}";
        public string FormattedDayAndDate => $"{CurrentDate.ToString("dddd dd")}";





        private DateTime CurrentDate
        {
            get { return _currentDate; }
            set
            {
                _currentDate = value;
                OnPropertyChanged();
            }
        }
        private string CurrentDayOfWeek
        {
            get { return _currentDayOfWeek; }
            set
            {
                _currentDayOfWeek = value;
                OnPropertyChanged();
            }
        }
        private int CurrentDay
        {
            get { return _currentDay; }
            set
            {
                _currentDay = value;
                OnPropertyChanged();
            }
        }
        private int CurrentMonth
        {
            get { return _currentMonth; }
            set
            {
                _currentMonth = value;
                OnPropertyChanged();
            }
        }
        private int CurrentYear
        {
            get { return _currentYear; }
            set
            {
                _currentYear = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            CurrentDate = DateTime.Now;
            CurrentDayOfWeek = CurrentDate.DayOfWeek.ToString();
            CurrentDay = CurrentDate.Day;
            CurrentMonth = CurrentDate.Month;
            CurrentYear = CurrentDate.Year;

            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}