using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Calendar
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DateTime _currentDate;
        private string _currentDayOfWeek;
        private int _currentDay;
        private int _currentMonth;
        private int _currentYear;

        public string FormattedDate => $"{CurrentDate.ToString("MMMM yyyy")}";




        private void PopulateCalendarGrid()
        {
            int daysInMonth = DateTime.DaysInMonth(CurrentDate.Year, CurrentDate.Month);
            DateTime firstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);

            calendarGrid.Children.Clear();

            // Display days from the previous month
            DateTime dayOfPreviousMonth = firstDayOfMonth.AddDays(-1);
            int numberOfDaysFromPreviousMonth = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;
            for (int i = numberOfDaysFromPreviousMonth - 1; i >= 0; i--)
            {
                AddDayToGrid(
                    dayOfPreviousMonth.Day.ToString(),
                    Brushes.LightGray,
                    i);
                dayOfPreviousMonth = dayOfPreviousMonth.AddDays(-1);
            }

            // Display days from the current month
            for (int i = 1; i <= daysInMonth; i++)
            {
                AddDayToGrid(
                    i.ToString(),
                    Brushes.Transparent,
                    i + numberOfDaysFromPreviousMonth - 1);
            }

            // Display days from the next month
            int remainingDays = 42 - numberOfDaysFromPreviousMonth - daysInMonth;
            for (int i = 1; i <= remainingDays; i++)
            {
                AddDayToGrid(
                    i.ToString(),
                    Brushes.LightGray,
                    numberOfDaysFromPreviousMonth + daysInMonth + i - 1);
            }
        }

        private void AddDayToGrid(string text, Brush background, int position)
        {
            TextBlock dayTextBlock = new TextBlock();
            dayTextBlock.Text = text;
            dayTextBlock.FontSize = 26;
            dayTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            dayTextBlock.VerticalAlignment = VerticalAlignment.Center;

            Border border = new Border();
            border.BorderBrush = Brushes.LightGray;
            border.BorderThickness = new Thickness(1);
            border.Background = background;
            border.Child = dayTextBlock;

            int row = position / 7;
            int column = position % 7;
            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);
            calendarGrid.Children.Add(border);
        }








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
            DataContext = this;
            PopulateCalendarGrid();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}