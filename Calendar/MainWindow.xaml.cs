using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



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

        public class CalendarEvent
        {
            public string Id { get; set; }
            public string Summary { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
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

            var BASE_CALENDAR_URL = "https://www.googleapis.com/calendar/v3/calendars";
            var CALENDAR_REGION = "no.norwegian";
            var BASE_CALENDAR_ID_FOR_PUBLIC_HOLIDAY = "holiday@group.v.calendar.google.com";
            var API_KEY = configuration["AbstractApi:ApiKey"];
            var HOLIDAYS_URL = $"{BASE_CALENDAR_URL}/{CALENDAR_REGION}%23{BASE_CALENDAR_ID_FOR_PUBLIC_HOLIDAY}/events?key={API_KEY}";

            // Sends a request to the api
            List<DateTime> holidayDates = await GetHolidayDates(HOLIDAYS_URL);

            DateTime selectedMonthFirstDay = new DateTime(CurrentYear, CurrentMonth, 1);
            DateTime previousMonthLastDay = selectedMonthFirstDay.AddDays(-1);

            int currentMonthDays = DateTime.DaysInMonth(CurrentYear, CurrentMonth);
            int previousMonthDays = ((int)selectedMonthFirstDay.DayOfWeek + 6) % 7;
            int remainingDays = 42 - previousMonthDays - currentMonthDays;

            calendarGrid.Children.Clear();
            calendarMonthYear.Text = $"{selectedMonthFirstDay.ToString("MMMM yyyy")}";

            // Display days from the previous month
            for (int i = previousMonthDays; i > 0; i--)
            {
                AddDayToGrid(
                    previousMonthLastDay.Day.ToString(),
                    Brushes.LightGray,
                    i - 1,
                    holidayDates);
                previousMonthLastDay = previousMonthLastDay.AddDays(-1);
            }

            // Display days from the current month
            for (int i = 1; i <= currentMonthDays; i++)
            {
                AddDayToGrid(
                    i.ToString(),
                    Brushes.Transparent,
                    i + previousMonthDays - 1,
                    holidayDates);
            }

            // Display days from the next month
            for (int i = 1; i <= remainingDays; i++)
            {
                AddDayToGrid(
                    i.ToString(),
                    Brushes.LightGray,
                    previousMonthDays + currentMonthDays + i - 1,
                    holidayDates);
            }
        }



        private void AddDayToGrid(string text, Brush background, int position, List<DateTime> holidayDates)
        {
            // Proceed with adding day to the grid
            TextBlock dayTextBlock = new TextBlock();
            dayTextBlock.Text = text;
            dayTextBlock.FontSize = 26;
            dayTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            dayTextBlock.VerticalAlignment = VerticalAlignment.Center;

            Border border = new Border();
            border.BorderBrush = Brushes.LightGray;
            border.BorderThickness = new Thickness(1);
            border.Child = dayTextBlock;

            // Calculate the date represented by the position
            DateTime firstDayOfMonth = new DateTime(CurrentYear, CurrentMonth, 1);
            int daysFromPreviousMonth = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;
            DateTime currentDate = firstDayOfMonth.AddDays(position - daysFromPreviousMonth);

            // Check if the day is a holiday
            bool isHoliday = holidayDates.Contains(currentDate.Date);

            // Testing proof of concept
            if (background == Brushes.Transparent)
                border.Background = isHoliday ? Brushes.Red : background;
            else if (background == Brushes.LightGray)
                border.Background = isHoliday ? Brushes.DarkGray : background;

            int row = position / 7;
            int column = position % 7;
            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);
            calendarGrid.Children.Add(border);
        }



        static async Task<List<DateTime>> GetHolidayDates(string apiUrl)
        {
            List<DateTime> eventDates = new List<DateTime>();

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(json);

                    foreach (var item in data["items"])
                    {
                        DateTime startDate = DateTime.Parse((string)item["start"]["date"]);
                        eventDates.Add(startDate);
                    }
                }
                else
                {
                    Console.WriteLine("Failed to retrieve data. Status code: " + response.StatusCode);
                }
            }

            return eventDates;
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