using System;
using System.Collections.Generic;
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
        private List<Event> events = new List<Event>();

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

        public class Holiday
        {
            public string? Summary { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
        public class Event
        {
            public string? Title { get; set; }
            public string? Description { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
        }

        public List<Event> TestEvents { get; set; }

        public MainWindow()
        {

            InitializeComponent();

            DataContext = this;

            // Sample events for testing purposes
            TestEvents = new List<Event>
            {
                new Event
                {
                    Title = "Meeting with Client",
                    Description = "Discuss project requirements",
                    Date = new DateTime(2024, 4, 17),
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(11, 0, 0)
                },
                new Event
                {
                    Title = "Team Lunch",
                    Description = "Celebrating a successful project",
                    Date = new DateTime(2024, 4, 18),
                    StartTime = new TimeSpan(12, 0, 0),
                    EndTime = new TimeSpan(13, 0, 0)
                },
            };


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
            List<Holiday> holidayDates = await GetHolidayDates(HOLIDAYS_URL);

            DateTime selectedMonthFirstDay = new DateTime(CurrentYear, CurrentMonth, 1);
            DateTime previousMonthLastDay = selectedMonthFirstDay.AddDays(-1);

            int currentMonthDays = DateTime.DaysInMonth(CurrentYear, CurrentMonth);
            int previousMonthDays = ((int)selectedMonthFirstDay.DayOfWeek + 6) % 7;
            int remainingDays = 42 - previousMonthDays - currentMonthDays;

            calendarGrid.Children.Clear();
            calendarMonthYear.Text = $"{selectedMonthFirstDay.ToString("MMMM yyyy")}";
            calendarDayMonth.Text = $"{CurrentDate.ToString("dddd dd MMMM yyyy")}";

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



        private void AddDayToGrid(string text, Brush background, int position, List<Holiday> holidayDates)
        {
            // Proceed with adding day to the grid
            TextBlock dayTextBlock = new TextBlock();
            dayTextBlock.Text = text;
            dayTextBlock.FontSize = 26;
            dayTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            dayTextBlock.VerticalAlignment = VerticalAlignment.Center;
            dayTextBlock.Padding = new Thickness(5);
            dayTextBlock.TextWrapping = TextWrapping.Wrap;

            Border border = new Border();
            border.BorderBrush = Brushes.LightGray;
            border.BorderThickness = new Thickness(1);
            border.Child = dayTextBlock;

            // Calculate the date represented by the position
            DateTime firstDayOfMonth = new DateTime(CurrentYear, CurrentMonth, 1);
            int daysFromPreviousMonth = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;
            DateTime currentDate = firstDayOfMonth.AddDays(position - daysFromPreviousMonth);

            // Check if the day is a holiday
            bool isHoliday = holidayDates.Any(eventItem =>
                eventItem.StartDate.Date == currentDate.Date
            );

            // Testing proof of concept
            if (background == Brushes.Transparent)
                border.Background = isHoliday ? Brushes.Red : background;
            else if (background == Brushes.LightGray)
                border.Background = isHoliday ? Brushes.DarkGray : background;

            if (isHoliday)
            {
                // Find the holiday event for the current date
                Holiday? holidayEvent = holidayDates.FirstOrDefault(eventItem =>
                    eventItem.StartDate.Date == currentDate.Date);

                // If a holiday event is found, set the text to its summary
                if (holidayEvent != null)
                {
                    dayTextBlock.Text = holidayEvent.Summary;
                    dayTextBlock.FontSize = 10;
                    dayTextBlock.FontWeight = FontWeights.SemiBold;
                    dayTextBlock.Foreground = Brushes.White;
                }
            }



            // Add click event handler to the day text block
            border.MouseLeftButtonDown += (sender, e) =>
            {

                // Handle day selection here
                DateTime selectedDate = currentDate;
                // Display selected day on the right-hand side
                calendarDayMonth.Text = selectedDate.ToString("dddd dd MMMM yyyy");
                border.Background = Brushes.Yellow;
            };



            int row = position / 7;
            int column = position % 7;
            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);
            calendarGrid.Children.Add(border);
        }



        //static async Task<List<CalendarEvent>> GetHolidayDates(string apiUrl)
        //{
        //    List<CalendarEvent> events = new List<CalendarEvent>();

        //    using (HttpClient client = new HttpClient())
        //    {
        //        HttpResponseMessage response = await client.GetAsync(apiUrl);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            string json = await response.Content.ReadAsStringAsync();
        //            JObject data = JObject.Parse(json);

        //            foreach (var item in data["items"]!)
        //            {
        //                string summary = (string)item["summary"]!;
        //                DateTime startDate = DateTime.Parse((string)item["start"]!["date"]!);
        //                DateTime endDate = DateTime.Parse((string)item["end"]!["date"]!);

        //                CalendarEvent calendarEvent = new CalendarEvent
        //                {
        //                    Summary = summary,
        //                    StartDate = startDate,
        //                    EndDate = endDate
        //                };

        //                events.Add(calendarEvent);
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine("Failed to retrieve data. Status code: " + response.StatusCode);
        //        }
        //    }

        //    return events;
        //}
        static async Task<List<Holiday>> GetHolidayDates(string apiUrl)
        {
            List<Holiday> events = new List<Holiday>
            {
                new Holiday
                {
                    Summary = "Placeholder Holiday",
                    StartDate = new DateTime(2024, 1, 15),
                    EndDate = new DateTime(2024, 1, 15)
                },                
                new Holiday
                {
                    Summary = "Placeholder Holiday",
                    StartDate = new DateTime(2024, 2, 15),
                    EndDate = new DateTime(2024, 2, 15)
                },                
                new Holiday
                {
                    Summary = "Placeholder Holiday",
                    StartDate = new DateTime(2024, 3, 15),
                    EndDate = new DateTime(2024, 3, 15)
                },                
                new Holiday
                {
                    Summary = "Placeholder Holiday",
                    StartDate = new DateTime(2024, 4, 15),
                    EndDate = new DateTime(2024, 4, 15)
                },                
                new Holiday
                {
                    Summary = "Placeholder Holiday",
                    StartDate = new DateTime(2024, 5, 15),
                    EndDate = new DateTime(2024, 5, 15)
                },                
                new Holiday
                {
                    Summary = "Placeholder Holiday",
                    StartDate = new DateTime(2024, 6, 15),
                    EndDate = new DateTime(2024, 6, 15)
                },                
                new Holiday
                {
                    Summary = "Placeholder Holiday",
                    StartDate = new DateTime(2024, 7, 15),
                    EndDate = new DateTime(2024, 7, 15)
                },                
                new Holiday
                {
                    Summary = "Placeholder Holiday",
                    StartDate = new DateTime(2024, 8, 15),
                    EndDate = new DateTime(2024, 8, 15)
                },                
                new Holiday
                {
                    Summary = "Placeholder Holiday",
                    StartDate = new DateTime(2024, 9, 15),
                    EndDate = new DateTime(2024, 9, 15)
                },                
                new Holiday
                {
                    Summary = "Placeholder Holiday",
                    StartDate = new DateTime(2024, 10, 15),
                    EndDate = new DateTime(2024, 10, 15)
                },                
                new Holiday
                {
                    Summary = "Placeholder Holiday",
                    StartDate = new DateTime(2024, 11, 15),
                    EndDate = new DateTime(2024, 11, 15)
                },                
                new Holiday
                {
                    Summary = "Placeholder Holiday",
                    StartDate = new DateTime(2024, 12, 15),
                    EndDate = new DateTime(2024, 12, 15)
                },
            };

            return events;
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
        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            // Create a new event based on input fields
            Event newEvent = new Event
            {
                Title = eventTitleTextBox.Text,
                Description = eventDescriptionTextBox.Text,
                Date = eventDatePicker.SelectedDate ?? DateTime.Today,
                StartTime = TimeSpan.Parse((eventStartTimeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()),
                EndTime = TimeSpan.Parse((eventEndTimeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString())
            };

            // Add the new event to the list of events
            TestEvents.Add(newEvent);

            // Refresh the display
            PopulateCalendarGrid();
        }

        private void ToggleAddEventPanelButton_Click(object sender, RoutedEventArgs e)
        {
            addEventPanel.Visibility = (addEventPanel.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            eventListDisplay.Visibility = (eventListDisplay.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
        }

    }
}