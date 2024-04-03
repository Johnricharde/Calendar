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
using System.Windows.Media;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;



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

            // Sends a request to the api
            //var norwegianHolidaysUrl = $"https://calendarific.com/api/v2/holidays?&api_key={apiKey}&country=NO&year={CurrentYear}&month={CurrentMonth}";
            //List<DateTime> holidayDates = await GetHolidayDates(norwegianHolidaysUrl);
            // Initialize a list of DateTimes for testing purposes
            List<DateTime> holidayDates = await GetHolidayDatesAsync();

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
                    holidayDates,
                    false);
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
                    holidayDates,
                    false);
            }
        }



        private void AddDayToGrid(string text, Brush background, int position, List<DateTime> holidayDates, bool isSelectedMonth=true)
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

            // Check if the day is a holiday
            //int daysInMonth = DateTime.DaysInMonth(CurrentYear, CurrentMonth);
            // Create DateTime object for the current day
            //DateTime currentDate = new DateTime(CurrentYear, CurrentMonth, daysInMonth);
            //bool isHoliday = holidayDates.Contains(currentDate.Date);
            //border.Background = isHoliday ? Brushes.Red : background;
            // Testing proof of concept
            if (background == Brushes.Transparent)
                border.Background = (text == "4" || text == "31") ? Brushes.Red : background;
            else if (background == Brushes.LightGray)
                border.Background = (text == "4" || text == "31") ? Brushes.DarkGray : background;

            int row = position / 7;
            int column = position % 7;
            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);
            calendarGrid.Children.Add(border);
        }



        private async Task<List<DateTime>> GetHolidayDates(string norwegianHolidaysUrl)
        {
            List<DateTime> holidayDates = new List<DateTime>();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(norwegianHolidaysUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        // Parse JSON response to extract holiday dates
                        dynamic holidaysData = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResponse);
                        if (holidaysData != null && holidaysData.response != null && holidaysData.response.holidays != null)
                        {
                            foreach (var holiday in holidaysData.response.holidays)
                            {
                                string dateString = holiday.date.iso;
                                DateTime holidayDate = DateTime.Parse(dateString, CultureInfo.InvariantCulture);
                                holidayDates.Add(holidayDate.Date);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to retrieve holiday data. Status code: " + response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while fetching holiday data: " + ex.Message);
            }

            return holidayDates;
        }

        // For testing purposes, DELETE THIS LATER
        private async Task<List<DateTime>> GetHolidayDatesAsync()
        {
            await Task.Delay(100); 
            return new List<DateTime>
            {
                new DateTime(2024, 3, 4),
                new DateTime(2024, 12, 25)
            };
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