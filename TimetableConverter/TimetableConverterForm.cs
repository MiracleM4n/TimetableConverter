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
using System.Reflection;
using System.Net;

namespace TimetableConverter
{
    public partial class frmTimetableConverter : Form
    {
        /*
         * 
         * CLASS DECLARATIONS
         * 
         */


        // Declare WebClient / Service
        RemoteWebDriver webClient;
        DriverService srvc;

        // Delcare work Thread
        BackgroundWorker scraperThread;
        BackgroundWorker downloadThread;

        // Declare File to save output to
        string file;

        // Declare debugging boolean
        bool debug = false;



        /*
         * 
         * ASYNC METHOD DECLARATION
         * 
         */


        // Declare Async Methods
        delegate void SetButtonTextCallback(string text);
        delegate void SetTextCallback(string text);
        delegate void AppendTextCallback(string text);
        delegate void ButtonEnableCallback(bool enabled);
        delegate string CampusSelectionCallback();



        /*
         * 
         * INITIALIZER
         * 
         */


        /// <summary>
        /// Form Initializer
        /// </summary>
        public frmTimetableConverter()
        {
            InitializeComponent();

            // Set Default File Location
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



        /*
         * 
         * EVENTS
         * 
         */


        /// <summary>
        /// Event Fired when output file location button is clicked
        /// Sets output file location and file name when the user clicks "Ok"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFileLocation_Click(object sender, EventArgs e)
        {
            // When the user clicks Ok on the file location dialog
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Set file variable to the filename
                file = saveFileDialog.FileName;
                // Set the file location textbox to the file
                txtFileLocation.Text = file;
            }
        }

        /// <summary>
        /// Fires when text is changed in the file location textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFileLocation_TextChanged(object sender, EventArgs e)
        {
            // Set the file to the next file location text
            file = txtFileLocation.Text;
        }

        /// <summary>
        /// Fires when the "Go" button is clicked
        /// Does validation than starts the "Work" Thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExport_Click(object sender, EventArgs e)
        {
            if (btnExport.Text.Contains("Download"))
            {

                downloadThread = new BackgroundWorker();

                downloadThread.DoWork += (s, args) =>
                {
                    this.setButtonEnabled(false);

                    doDownloadWork();
                };

                downloadThread.RunWorkerCompleted += (s, args) =>
                {
                    this.setText("This utility will scrape the DC / UOIT MyCampus website for Timetable data then export it into a format that is readable by many Calendar programs including Google Calendar. " +
                                 "Enter your DC / UOIT MyCampus login details, select your campus, then select the file location, finally hit the export button.\n");

                    this.setButtonText("&Export my Calendar!");

                    this.setButtonEnabled(true);
                };

                // Start the worker
                downloadThread.RunWorkerAsync();
            }
            else
            {
                // Checks if the user has set a output file
                if (file == null)
                {
                    // If not prompt the user to enter one
                    System.Windows.Forms.MessageBox.Show("You need to select a destination file path first.");
                    // return (Don't start "Work" Thread)
                    return;
                }

                // Define the thread variable using a new Thread from current user input
                scraperThread = new BackgroundWorker();

                scraperThread.DoWork += (s, args) =>
                {
                    // Disable the output button until "Work" Thread is complete
                    setButtonEnabled(false);

                    doScrapingWork();
                };

                // Start the worker
                scraperThread.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Fires when Form is Closed
        /// Aborts Thead / Quits WebClient
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmTemperatureConversion_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Check if the scraper thread has been initialized and is currently running
            if (this.scraperThread != null && this.scraperThread.IsBusy)
            {
                // Abort the thread if so
                scraperThread.CancelAsync();
            }

            // Check if the download thread has been initialized and is currently running
            if (this.downloadThread != null && this.downloadThread.IsBusy)
            {
                // Abort the thread if so
                downloadThread.CancelAsync();
            }

            // Check if the webClient has been initialized
            if (this.webClient != null)
            {
                // If so Quit it (Close it)
                webClient.Quit();
            }
        }

        /// <summary>
        /// Event fired when form has "Activated"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmTimetableConverter_Activated(object sender, EventArgs e)
        {
            if (!((debug ? checkDependency("chromedriver.exe") : checkDependency("phantomjs.exe"))
                 && checkDependency("DDay.iCal.dll")
                 && checkDependency("WebDriver.dll")))
            {
                tbxMain.Text = "Dependencies required before this program can be used. Click the download button to download them.";
                btnExport.Text = "&Download Dependencies";
            }
        }



        /*
         * 
         * METHODS
         * 
         */


        /// <summary>
        /// Main "Work" Thread
        /// Does all scraping work. To be used in another Thread to prevent Form locking.
        /// </summary>
        private void doScrapingWork()
        {
            // Clear output window
            this.setText("");

            // Check if the webClient has already been initialized
            if (this.webClient != null)
            {
                // If so close WebClient
                this.appendText("Closing WebClient!" + Environment.NewLine);
                webClient.Quit();
            }

            // Checks if Debug mode boolean is set
            if (debug)
            {
                // Set Debug WebClient if so

                // Set WebClient Service options
                srvc = ChromeDriverService.CreateDefaultService();
                srvc.HideCommandPromptWindow = true;

                // Initialize WebClient
                this.appendText("Starting WebClient" + Environment.NewLine);
                webClient = new ChromeDriver((ChromeDriverService)srvc);
            }
            else
            {
                // Set Regular WebClient if not

                // Set WebClient Service options
                srvc = PhantomJSDriverService.CreateDefaultService();
                srvc.HideCommandPromptWindow = true;

                // Set WebClient options
                PhantomJSOptions opts = new PhantomJSOptions();
                opts.AddAdditionalCapability("web-security", false);
                opts.AddAdditionalCapability("ssl-protocol", "any");
                opts.AddAdditionalCapability("ignore-ssl-errors", true);
                opts.AddAdditionalCapability("webdriver-loglevel", "DEBUG");

                // Initialize WebClient
                this.appendText("Starting WebClient" + Environment.NewLine);
                webClient = new PhantomJSDriver((PhantomJSDriverService)srvc, opts);
            }

            // Set WebClient pag load timeout to 10 seconds
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

            switch (this.getCampusSelection())
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

                // Enable go button
                this.setButtonEnabled(true);

                // End Thread
                return;
            }

            // Select / Click Timetable Anchor
            this.appendText("Clicking 'Student schedule' link" + Environment.NewLine);
            webClient.FindElement(By.LinkText("Student schedule by day and time")).Click();

            // Select Timetable IFrame
            this.appendText("Switch to Content IFrame" + Environment.NewLine);
            webClient.SwitchTo().Frame("content");

            // Select / Click Second Timetable Anchor 
            this.appendText("Clicking Second 'Student schedule' link" + Environment.NewLine);
            webClient.FindElement(By.PartialLinkText("Student Schedule")).Click();

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
                        String[] classArray = room.Split(' ');
                        String title = clss + "(" + classArray[classArray.Length - 1] + ")";
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
            this.appendText(ical.Calendar.Events.Count + " events being written to file" + Environment.NewLine + "'" + txtFileLocation.Text + "'" + Environment.NewLine);
            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.Serialize(ical, txtFileLocation.Text);

            // Quite WebClient
            this.appendText("Closing WebClient!" + Environment.NewLine);
            webClient.Quit();

            // Enable Go button
            this.setButtonEnabled(true);
        }

        /// <summary>
        /// Download "Work" Thread
        /// Does all dependency downloading work. To be used in another Thread to prevent Form locking.
        /// </summary>
        private void doDownloadWork()
        {
            this.setText("");

            // Check / Download dependencies as needed
            if (debug)
            {
                this.checkAndDownloadDependency("chromedriver.exe");
            }
            else
            {
                this.checkAndDownloadDependency("phantomjs.exe");
            }

            this.checkAndDownloadDependency("DDay.iCal.dll");
            this.checkAndDownloadDependency("WebDriver.dll");
        }

        /// <summary>
        // Add event to provided calendar
        /// </summary>
        /// <param name="ical">Calendar to add event to</param>
        /// <param name="startDate">Start date/time of event</param>
        /// <param name="endDate">End date/time of event</param>
        /// <param name="description">Description of event</param>
        /// <param name="title">Title of event</param>
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
            evt.Location = getCampusSelection() + ", Oshawa, ON, Canada";

            // Create and set UUID for event
            Guid uuid = System.Guid.NewGuid();
            string uuidValue = uuid.ToString().Replace("-", "").Substring(0, 26) + "@durhamcollege.ca";
            evt.UID = uuidValue;
        }

        /// <summary>
        /// Checks if dependency is available
        /// </summary>
        /// <param name="dependency">Dependency to check for</param>
        /// <returns>Whether the dependency is found or not</returns>
        private bool checkDependency(string dependency)
        {
            return File.Exists(dependency);
        }

        /// <summary>
        /// Checks if dependency is available, Downloads it if not
        /// </summary>
        /// <param name="dependency">Dependency to check for</param>
        private void checkAndDownloadDependency(string dependency)
        {
            if (!checkDependency(dependency))
            {
                System.Net.WebClient downloadClient = new System.Net.WebClient();

                this.appendText("Downloading dependency \"" + dependency + "\". \n");

                downloadClient.DownloadProgressChanged += (s, e) =>
                {
                    this.appendText(dependency + ": " + e.BytesReceived + " of " + e.TotalBytesToReceive + " bytes. " + e.ProgressPercentage + "% complete. \n");
                };

                downloadClient.DownloadFileCompleted += (s, e) =>
                {
                    this.appendText("Dependency \"" + dependency + "\" downloaded successfully!\n");

                    string[] splitName = dependency.Split('.');

                    string extension = splitName[splitName.Length - 1];

                    if (extension == "dll")
                    {
                        Assembly assembly = Assembly.LoadFrom(dependency);

                        AppDomain.CurrentDomain.Load(assembly.GetName());
                    }
                };

                downloadClient.DownloadFileAsync(new Uri("http://vps.q0r.ca/tc/" + dependency), dependency);
            }
        }



        /*
         * 
         * ASYNC METHODS
         * 
         */


        /// <summary>
        /// Sets main button text value Async
        /// </summary>
        /// <param name="text">Value to set button text to</param>
        private void setButtonText(string text)
        {
            if (this.btnExport.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(setButtonText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.btnExport.Text = text;
            }
        }

        /// <summary>
        /// Sets main textbox text value Async
        /// </summary>
        /// <param name="text">Value to set textbox to</param>
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

        /// <summary>
        /// Appends text to main textbox Async
        /// </summary>
        /// <param name="text">Value to append to textbox</param>
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

        /// <summary>
        /// Sets whether the "Export" button can be clicked Async
        /// </summary>
        /// <param name="enabled">Value of enabled status on button</param>
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

        /// <summary>
        /// Get's current campus selection Async
        /// </summary>
        /// <returns>Current campus selection value</returns>
        private string getCampusSelection()
        {
            if (this.cbxCampus.InvokeRequired)
            {
                CampusSelectionCallback d = new CampusSelectionCallback(getCampusSelection);
                return (string)this.Invoke(d);
            }
            else
            {
                return (string)cbxCampus.SelectedValue;
            }
        }
    }
}
