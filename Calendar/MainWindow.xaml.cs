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
            DayOfWeek firstDayOfWeek = firstDayOfMonth.DayOfWeek;
            //int startingColumn = (int)firstDayOfWeek;

            calendarGrid.Children.Clear();

            // PREVIOUS MONTH //////////////////////////////////////////////////////////////////////////////
            // Get the last day of the previous month
            DateTime lastDayOfPreviousMonth = firstDayOfMonth.AddDays(-1);
            // Calculate the number of days from the previous month to display
            int daysFromPreviousMonth = ((int)firstDayOfWeek + 6) % 7;   
            // Populates the calendar grid with the days of the previous month
            for (int i = daysFromPreviousMonth - 1; i >= 0; i--) 
            {
                TextBlock dayTextBlock = new TextBlock();
                dayTextBlock.Text = lastDayOfPreviousMonth.Day.ToString();
                dayTextBlock.FontSize = 26;
                dayTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                dayTextBlock.VerticalAlignment = VerticalAlignment.Center;

                // Create border styling for days from previous month
                Border border = new Border(); 
                border.BorderBrush = Brushes.LightGray;  
                border.BorderThickness = new Thickness(1);
                border.Background = Brushes.LightGray;   
                border.Child = dayTextBlock;       

                // Calculate the row and column for the current day
                int row = i / 7;
                int column = i % 7;
                // Add the border to the appropriate cell in the grid
                Grid.SetRow(border, row);
                Grid.SetColumn(border, column);
                calendarGrid.Children.Add(border);
                // Decrement the day for the next iteration
                lastDayOfPreviousMonth = lastDayOfPreviousMonth.AddDays(-1);
            }

            // CURRENT MONTH ///////////////////////////////////////////////////////////////////////////////
            // Populates the calendar grid with the days of the current month
            for (int day = 1; day <= daysInMonth; day++)
            {
                TextBlock dayTextBlock = new TextBlock();
                dayTextBlock.Text = day.ToString();
                dayTextBlock.FontSize = 26;
                dayTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                dayTextBlock.VerticalAlignment = VerticalAlignment.Center;

                // Create border styling for days from current month
                Border border = new Border();
                border.BorderBrush = Brushes.LightGray;
                border.BorderThickness = new Thickness(1);
                border.Child = dayTextBlock;

                // Calculate the row and column for the current day
                int row = (day + daysFromPreviousMonth - 1) / 7;
                int column = (day + daysFromPreviousMonth - 1) % 7;
                // Add the border to the appropriate cell in the grid
                Grid.SetRow(border, row);
                Grid.SetColumn(border, column);
                calendarGrid.Children.Add(border);
            }

            // NEXT MONTH //////////////////////////////////////////////////////////////////////////////////
            // Calculate the number of days from the next month to display
            int remainingDays = 42 - daysFromPreviousMonth - daysInMonth;
            // Populates the calendar grid with the days of the next month
            for (int i = 1; i <= remainingDays; i++)
            {
                TextBlock dayTextBlock = new TextBlock();
                dayTextBlock.Text = i.ToString();
                dayTextBlock.FontSize = 26;
                dayTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                dayTextBlock.VerticalAlignment = VerticalAlignment.Center;

                // Create border styling for days from next month
                Border border = new Border();
                border.BorderBrush = Brushes.LightGray;
                border.BorderThickness = new Thickness(1);
                border.Background = Brushes.LightGray;
                border.Child = dayTextBlock;

                // Calculate the row and column for the current day
                int row = (daysFromPreviousMonth + daysInMonth + i - 1) / 7;
                int column = (daysFromPreviousMonth + daysInMonth + i - 1) % 7;
                // Add the border to the appropriate cell in the grid
                Grid.SetRow(border, row);
                Grid.SetColumn(border, column);
                calendarGrid.Children.Add(border);
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}