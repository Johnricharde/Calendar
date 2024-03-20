using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Calendar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DateTime _currentDate;
        private string _currentDayOfWeek;
        private int _currentDay;
        private int _currentMonth;
        private int _currentYear;

        public DateTime CurrentDate
        {
            get { return _currentDate; }
            set
            {
                _currentDate = value;
                OnPropertyChanged();
            }
        }
        public string CurrentDayOfWeek
        {
            get { return _currentDayOfWeek; }
            set
            {
                _currentDayOfWeek = value;
                OnPropertyChanged();
            }
        }
        public int CurrentDay
        {
            get { return _currentDay; }
            set
            {
                _currentDay = value;
                OnPropertyChanged();
            }
        }
        public int CurrentMonth
        {
            get { return _currentMonth; }
            set
            {
                _currentMonth = value;
                OnPropertyChanged();
            }
        }
        public int CurrentYear
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
            DataContext = this; // Set the DataContext to this MainWindow instance
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}