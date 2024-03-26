using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.Configuration;



namespace Calendar
{
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
            set { _currentDate = value; OnPropertyChanged(); }
        }
        public string CurrentDayOfWeek
        {
            get { return _currentDayOfWeek; }
            set { _currentDayOfWeek = value; OnPropertyChanged(); }
        }
        public int CurrentDay
        {
            get { return _currentDay; }
            set { _currentDay = value; OnPropertyChanged(); }
        }
        public int CurrentMonth
        {
            get { return _currentMonth; }
            set { _currentMonth = value; OnPropertyChanged(); }
        }
        public int CurrentYear
        {
            get { return _currentYear; }
            set { _currentYear = value; OnPropertyChanged(); }
        }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Set default values for CurrentDate
            CurrentDate = DateTime.Today;

            // Set CurrentYear and CurrentMonth based on CurrentDate
            CurrentYear = CurrentDate.Year;
            CurrentMonth = CurrentDate.Month;



            PopulateCalendarGrid();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        private async void PopulateCalendarGrid()
        {
            // Read API key from appsettings.json
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            string apiKey = configuration["AbstractApi:ApiKey"];

            var norwegianHolidaysUrl = $"https://calendarific.com/api/v2/holidays?&api_key={apiKey}&country=NO&year={CurrentYear}&month={CurrentMonth}";
            List<DateTime> holidayDates = await GetHolidayDates(norwegianHolidaysUrl);

            //DateTime firstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);

            // Everything should be built around this line
            DateTime firstDayOfSelectedMonth = new DateTime(CurrentYear, CurrentMonth, 1);

            calendarMonthYear.Text = $"{firstDayOfSelectedMonth.ToString("MMMM yyyy")}";

            int daysInMonth = DateTime.DaysInMonth(CurrentYear, CurrentMonth);

            calendarGrid.Children.Clear();


            // Display days from the previous month
            DateTime dayOfPreviousMonth = firstDayOfSelectedMonth.AddDays(-1);
            int numberOfDaysFromPreviousMonth = ((int)firstDayOfSelectedMonth.DayOfWeek + 6) % 7;
            for (int i = numberOfDaysFromPreviousMonth - 1; i >= 0; i--)
            {
                AddDayToGrid(
                    dayOfPreviousMonth.Day.ToString(),
                    Brushes.LightGray,
                    i,
                    holidayDates);
                dayOfPreviousMonth = dayOfPreviousMonth.AddDays(-1);
            }

            // Display days from the current month
            for (int i = 1; i <= daysInMonth; i++)
            {
                AddDayToGrid(
                    i.ToString(),
                    Brushes.Transparent,
                    i + numberOfDaysFromPreviousMonth - 1,
                    holidayDates);
            }

            // Display days from the next month
            int remainingDays = 42 - numberOfDaysFromPreviousMonth - daysInMonth;
            for (int i = 1; i <= remainingDays; i++)
            {
                AddDayToGrid(
                    i.ToString(),
                    Brushes.LightGray,
                    numberOfDaysFromPreviousMonth + daysInMonth + i - 1,
                    holidayDates);
            }
        }

        private void AddDayToGrid(string text, Brush background, int position, List<DateTime> holidayDates)
        {
            TextBlock dayTextBlock = new TextBlock();
            dayTextBlock.Text = text;
            dayTextBlock.FontSize = 26;
            dayTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            dayTextBlock.VerticalAlignment = VerticalAlignment.Center;

            Border border = new Border();
            border.BorderBrush = Brushes.LightGray;
            border.BorderThickness = new Thickness(1);
            border.Child = dayTextBlock;

            // Check if the day is a holiday
            DateTime currentDate = DateTime.Parse(text);
            bool isHoliday = holidayDates.Contains(currentDate.Date);
            border.Background = isHoliday ? Brushes.Red : background;
            int row = position / 7;
            int column = position % 7;
            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);
            calendarGrid.Children.Add(border);
        }

        private async Task<List<DateTime>> GetHolidayDates(string norwegianHolidaysUrl)
        {
            // Call the API to fetch holiday data for the month or a range of months
            // Parse the response and extract holiday dates
            // Return the list of holiday dates

            // Example pseudo-code:
            // Call the API using norwegianHolidaysUrl
            // Parse the response to extract holiday dates
            // Return the list of holiday dates
        }

        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMonth++;
            if (CurrentMonth > 12)
            {
                CurrentMonth = 1;
                CurrentYear++;
            }
            PopulateCalendarGrid();
        }
        private void PreviousMonthButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMonth--;
            if (CurrentMonth < 1)
            {
                CurrentMonth = 12;
                CurrentYear--;
            }
            PopulateCalendarGrid();
        }
    }
}