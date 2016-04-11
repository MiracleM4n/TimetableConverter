using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;
using System.Threading;

namespace TimetableConverter
{
    public partial class frmTemperatureConversion : Form
    {
        string file;

        public frmTemperatureConversion()
        {
            InitializeComponent();

            file = System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Desktop\\calendar.ics";
            txtFileLocation.Text = file;

            // Set Save File Dialog options
            saveFileDialog.Filter = "Calendar Files | *.ics, *.vcs";
            saveFileDialog.AddExtension = true;
            saveFileDialog.InitialDirectory = System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Desktop\\";
            saveFileDialog.DefaultExt = "ics";
            saveFileDialog.FileName = "calendar.ics";

            // Set default Dropdown Selection
            cbxCampus.SelectedItem = "Durham College";
        }

        delegate void SetTextCallback(string text);
        delegate void AppendTextCallback(string text);
        delegate void ButtonEnableCallback(bool enabled);

        private void btnFileLocation_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                file = saveFileDialog.FileName;
                txtFileLocation.Text = file;
            }
        }

        private void txtFileLocation_TextChanged(object sender, EventArgs e)
        {
            file = txtFileLocation.Text;
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            if (file == null)
            {
                System.Windows.Forms.MessageBox.Show("You need to select a destination file path first.");
                return;
            }

            Thread thread = new Thread(new ThreadStart(() => doWork(tbxMain, txtUsername, txtPassword)));

            btnExport.Enabled = false;

            // Start the thread
            thread.Start();
        }

        private void doWork(RichTextBox tboxMain, TextBox user, TextBox password)
        {
            // Set WebClient options
            PhantomJSDriverService srvc = PhantomJSDriverService.CreateDefaultService();
            srvc.HideCommandPromptWindow = true;

            PhantomJSOptions opts = new PhantomJSOptions();
            opts.AddAdditionalCapability("web-security", false);
            opts.AddAdditionalCapability("ssl-protocol", "any");
            opts.AddAdditionalCapability("ignore-ssl-errors", true);
            opts.AddAdditionalCapability("webdriver-loglevel", "DEBUG");

            // Initialize WebClient

            this.setText("Starting WebClient" + Environment.NewLine);

            PhantomJSDriver webClient = new PhantomJSDriver(srvc, opts);
            webClient.Manage().Timeouts().SetPageLoadTimeout(new TimeSpan(0, 0, 10));


            // Open DC MyCampus login page
            this.appendText("Loading MyCampus login page" + Environment.NewLine);
            webClient.Navigate().GoToUrl("http://www.durhamcollege.ca/mycampus/");

            // Pass username and password to page
            this.appendText("Inputting login details" + Environment.NewLine);
            webClient.ExecuteScript("document.cplogin.user.value='" + txtUsername.Text + "';");
            webClient.ExecuteScript("document.cplogin.pass.value='" + txtPassword.Text + "';");

            // Click login button
            this.appendText("Logging in" + Environment.NewLine);
            webClient.FindElement(By.Id("submit-text-search")).Click();

            // Set Campus based on ComboBox
            string homeAnchorText;
            
            switch ((string) cbxCampus.SelectedValue) 
            {
                case "Durham College":
                    homeAnchorText = "DC Home";
                    break;
                case "UOIT":
                    homeAnchorText = "UOIT Home";
                    break;
                default:
                    homeAnchorText = "DC Home";
                    break;
            }

            // Search for Campus "Home" Anchor
            IReadOnlyCollection<IWebElement> homeAnchor = webClient.FindElements(By.LinkText(homeAnchorText));

            // Check if login succeeded based on the "Home" Anchor
            if (homeAnchor.Count > 0)
            {
                // Click Anchor to load Student Information
                webClient.FindElement(By.LinkText(homeAnchorText)).Click();

                this.appendText("Login successful" + Environment.NewLine);
            }
            else
            {
                // Login unsuccessful
                this.appendText("Login Unsuccessful!" + Environment.NewLine);

                this.appendText("Webpage: " + webClient.PageSource + Environment.NewLine);

                // Close WebClient
                this.appendText("Closing WebClient!" + Environment.NewLine);
                webClient.Quit();

                // Enable go button
                this.setButtonEnabled(true);

                return;
            }

            // Select / Click Timetable Anchor
            this.appendText("Clicking 'Student schedule' link" + Environment.NewLine);
            webClient.FindElement(By.LinkText("Student schedule by day and time")).Click();

            // Select Timetable IFrame
            this.appendText("Switch to Timetable IFrame" + Environment.NewLine);
            webClient.SwitchTo().Frame("content");

            // Create empty Calendar with default settings
            this.appendText("Initialize blank ICalendar" + Environment.NewLine);
            iCalendar ical = new iCalendar();

            // Set required properties to the Calendar
            this.appendText("Set ICalendar properties" + Environment.NewLine);
            ical.ProductID = "-//Scott Chi//iCalendar 1.0//EN";
            ical.Version = "2.0";
            ical.Scale = "GREGORIAN";

            // Initialize 
            this.appendText("Initialize Dictionary" + Environment.NewLine);
            Dictionary<string, Dictionary<string, string>> calendarMap = new Dictionary<string, Dictionary<string, string>>();

            int lastPage = 0;

            // Add all classes in the week
            this.appendText("Start grabbing class info from each week" + Environment.NewLine);
            do
            {
                // Grab Weekstart from Timetable IFrame
                String weekStart = webClient.FindElement(By.XPath("//span[@class='fieldlargetext']")).Text.Substring(8);

                // Loop through Timetable Anchors
                foreach (var calendarAnchor in webClient.FindElements(By.XPath("//table[@class='bordertable']/tbody//tr//td//a")))
                {
                    // Grab class from Anchor
                    string clss = calendarAnchor.Text.Split('\r')[0];
                    // Grab Anchor Link from Anchor
                    string href = calendarAnchor.GetAttribute("href");

                    // Check if there are keys for this week yet
                    if (!calendarMap.ContainsKey(weekStart))
                    {
                        // If not initialize week in Dictionary
                        calendarMap.Add(weekStart, new Dictionary<string, string>());
                    }

                    // Check if the class has already been added to the week entry
                    if (calendarMap.ContainsKey(weekStart) && calendarMap[weekStart].ContainsKey(clss))
                    {
                        // If so skip it
                        continue;
                    }

                    // Add class to week
                    calendarMap[weekStart].Add(clss, href);
                }

                IReadOnlyCollection<IWebElement> nextPage = webClient.FindElements(By.XPath("//table[@class='plaintable']/tbody//tr//p[@class='rightaligntext']//a"));

                // Check if this is last page
                if (nextPage.Count > 0)
                {
                    // If not click to next page
                    nextPage.First().Click();
                }

                // Check if next week will be last week
                if (webClient.FindElements(By.XPath("//table[@class='plaintable']/tbody//tr//p[@class='rightaligntext']//a")).Count == 0)
                {
                    // If so increment the last page (only works once)
                    lastPage++;
                    continue;
                }
            }
            // While there is a "Next Week" or if page is the "Last Page" anchor add all classes in week
            while (webClient.FindElements(By.XPath("//table[@class='plaintable']/tbody//tr//p[@class='rightaligntext']//a")).Count > 0 || lastPage == 1);

            // Intitilize week count
            int weekCount = 1;

            // Loop through weeks
            this.appendText("Loop through grabbed data" + Environment.NewLine);
            foreach (KeyValuePair<string, Dictionary<string, string>> dictEntry in calendarMap)
            {
                this.appendText("Parsing information from week " + weekCount + Environment.NewLine);

                // Grab week start date
                string weekStart = dictEntry.Key;

                // Loop through classes
                foreach (KeyValuePair<string, string> entry in dictEntry.Value)
                {
                    // Initialize class name
                    string clss = entry.Key;

                    // Go to class url
                    webClient.Navigate().GoToUrl(entry.Value);

                    // Initialize time, room, teacherName variables
                    string time = "", room = "", teacherName = "";

                    // Loop through Timetable rows
                    foreach (var row in webClient.FindElements(By.XPath("//table[@class='bordertable' and caption/.='Scheduled Meeting Times']/tbody//tr")))
                    {
                        // Check if row is header row or if this class has already been added if either is true continue loop
                        if (row.Text.Contains("Type"))
                        {
                            continue;
                        }

                        // Initialize row counter and date advancer
                        int i = 0, advDate = 0;

                        // Loop through row cells
                        foreach (var dataCell in row.FindElements(By.TagName("td")))
                        {
                            // Switch that checks which column the data corresponds to
                            switch (i)
                            {
                                // Type of class, not currently used
                                case 0:
                                    break;
                                // Time field
                                case 1:
                                    time = dataCell.Text;
                                    break;
                                // Days of week field
                                case 2:
                                    // Initialize dayOfWeek variable
                                    String dayOfWeek = dataCell.Text;
                                    // Switch that checks which day of the week it is
                                    // Changes date advancer to match day of week
                                    // e.g. Monday = no advancement, Tuesday = one day advancement
                                    switch (dayOfWeek)
                                    {
                                        case "M":
                                            break;
                                        case "T":
                                            advDate = 1;
                                            break;
                                        case "W":
                                            advDate = 2;
                                            break;
                                        case "R":
                                            advDate = 3;
                                            break;
                                        case "F":
                                            advDate = 4;
                                            break;
                                    }
                                    break;
                                // Room field
                                case 3:
                                    room = dataCell.Text;
                                    break;
                                // Date range field, not currently in use
                                case 4:
                                    break;
                                // Schedule type field, not currently used
                                case 5:
                                    //scheduleType = dataCell.getTextContent();
                                    break;
                                // Teacher name field
                                case 6:
                                    teacherName = dataCell.Text.Split('(')[0].Trim();
                                    break;
                            }

                            // Increment row counter
                            i++;
                        }

                        // Create the Events title class name with room number in parentheses
                        String title = clss + "(" + room.Split(' ')[1] + ")";
                        // Create the Events description each item is on a new line
                        // Includes class name, time of class, room number, teachers name
                        String description = clss + Environment.NewLine + time + Environment.NewLine + room + Environment.NewLine + teacherName;

                        // Create a Date formatter that will be used to format a time string
                        //string format = "MMM dd, yyyy hh:mm aa";

                        // Split the time range
                        String[] splitTime = time.Split('-');

                        // Add the start and end times to the date, parse it and convert it to a Date object
                        DateTime startDate;
                        DateTime endDate;

                        if (!(DateTime.TryParse(weekStart + " " + splitTime[0].Trim(), out startDate) &&
                        DateTime.TryParse(weekStart + " " + splitTime[1].Trim(), out endDate)))
                        {
                            //TODO Print Message.
                            return;
                        }

                        // Add the date advance to the date
                        // 86400000 milliseconds = 1 day = 24 hours
                        DateTime startDateTime = startDate.AddDays(advDate);
                        DateTime endDateTime = endDate.AddDays(advDate);

                        // Pass variables to add event to calendar function
                        addEventToCalendar(ical, startDateTime, endDateTime, description, title);
                    }
                }

                this.appendText(dictEntry.Value.Count + " events added from week " + weekCount + Environment.NewLine);

                weekCount++;
            }

            // Write calendar to file
            this.appendText(ical.Calendar.Events.Count + " events being written to file" + Environment.NewLine + "'" + txtFileLocation.Text +  "'" + Environment.NewLine);
            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.Serialize(ical, txtFileLocation.Text);

            // Quite WebClient
            this.appendText("Closing WebClient!" + Environment.NewLine);
            webClient.Quit();

            // Enable Go button
            this.setButtonEnabled(true);
        }

        
        // Add event to provided calendar
        // Requires event start and end dates, event description and event title/summary
        private void addEventToCalendar(iCalendar ical, DateTime startDate, DateTime endDate, string description, string title) 
        {
            // Create a blank event with default settings
            Event evt = ical.Create<Event>();

            // Set the title/summary
            evt.Summary = title;

            // Set event start and end dates
            evt.Start = new iCalDateTime(startDate);
            evt.End = new iCalDateTime(endDate);

            // Set event description
            evt.Description = description;
            // Set event location
            evt.Location = "Durham College, Oshawa, ON, Canada";

            // Create and set UUID for event
            Guid uuid = System.Guid.NewGuid();
            string uuidValue = uuid.ToString().Replace("-", "").Substring(0, 26) + "@durhamcollege.ca";
            evt.UID = uuidValue;
        }

        private void setText(string text)
        {
            if (this.tbxMain.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(setText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.tbxMain.Text = text;
            } 
        }

        private void appendText(string text)
        {
            if (this.tbxMain.InvokeRequired)
            {
                AppendTextCallback d = new AppendTextCallback(appendText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.tbxMain.AppendText(text);
            } 
        }

        private void setButtonEnabled(bool enabled)
        {
            if (this.btnExport.InvokeRequired)
            {
                ButtonEnableCallback d = new ButtonEnableCallback(setButtonEnabled);
                this.Invoke(d, new object[] { enabled });
            }
            else
            {
                this.btnExport.Enabled = enabled;
            } 
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
