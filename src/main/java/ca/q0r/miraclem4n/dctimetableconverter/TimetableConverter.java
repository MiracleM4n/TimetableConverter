package ca.q0r.miraclem4n.dctimetableconverter;

import biweekly.Biweekly;
import biweekly.ICalVersion;
import biweekly.ICalendar;
import biweekly.component.VEvent;
import biweekly.property.CalendarScale;
import biweekly.property.Description;
import biweekly.property.ProductId;
import biweekly.property.Uid;
import com.gargoylesoftware.htmlunit.BrowserVersion;
import com.gargoylesoftware.htmlunit.WebClient;
import com.gargoylesoftware.htmlunit.html.*;

import java.io.File;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.*;

public class TimetableConverter {
    public static void main(String[] args) {
        try {
            // Initialize WebClient
            WebClient webClient = new WebClient(BrowserVersion.CHROME);

            // General WebClient settings that make everything work with DC MyCampus
            webClient.getCookieManager().setCookiesEnabled(true);
            webClient.getOptions().setThrowExceptionOnFailingStatusCode(false);
            webClient.getOptions().setThrowExceptionOnScriptError(false);
            webClient.getOptions().setUseInsecureSSL(true);
            webClient.getOptions().setJavaScriptEnabled(true);
            webClient.getOptions().setSSLInsecureProtocol("TLSv1");

            webClient.getOptions().setPrintContentOnFailingStatusCode(false);

            // Open DC MyCampus login page
            HtmlPage loginPage = webClient.getPage("http://www.durhamcollege.ca/mycampus/");

            // Pass username and password to page
            loginPage.executeJavaScript("document.cplogin.user.value='" + args[0] + "';");
            loginPage.executeJavaScript("document.cplogin.pass.value='" + args[1] + "';");

            // Click login button
            HtmlPage portalPage = loginPage.getElementById("submit-text-search").click();

            // Select Timetable Anchor
            HtmlAnchor anchor = portalPage.getAnchorByText("Student schedule by day and time");

            // Click Anchor
            HtmlPage timeTablePage = anchor.click();

            // Select Timetable IFrame
            HtmlPage calendarPage = (HtmlPage) timeTablePage.getFrameByName("content").getEnclosedPage();

            // Create empty Calendar with default settings
            ICalendar ical = new ICalendar();

            // Set required properties to the Calendar
            ical.setProductId(new ProductId("-//Scott Chi//biweekly 1.0//EN"));
            ical.setVersion(ICalVersion.V2_0);
            ical.setCalendarScale(CalendarScale.gregorian());

            // Grab Weekstart from Timetable IFrame
            String weekStart = ((HtmlSpan) calendarPage.getByXPath("//span[@class='fieldlargetext']").get(0)).getTextContent().substring(8);

            // Initialize Array so that entries are not duplicated
            List<String> clssList = new ArrayList<>();

            // Loop through Timetable Anchors
            for (HtmlAnchor calendarAnchor : (List<HtmlAnchor>) calendarPage.getByXPath("//table[@class='bordertable']/tbody//tr//td//a")) {
                // Open Timetable description page
                HtmlPage coursePage = webClient.getPage("https://ssbp.mycampus.ca" + calendarAnchor.getHrefAttribute());

                // Grab the name of the class
                String clss = calendarAnchor.getChildNodes().get(0).asText();
                // Initialize time, room, teacherName variables
                String time = "", room = "", teacherName = "";

                // Loop through Timetable rows
                for (HtmlTableRow row : (List<HtmlTableRow>) coursePage.getByXPath("//table[@class='bordertable' and caption/.='Scheduled Meeting Times']/tbody//tr")) {
                    // Check if row is header row or if this class has already been added if either is true continue loop
                    if (row.asText().contains("Type") || clssList.contains(clss)) {
                        continue;
                    }

                    // Initialize row counter and date advancer
                    int i = 0, advDate = 0;

                    // Loop through row cells
                    for (HtmlTableCell dataCell : row.getCells()) {
                        // Switch that checks which column the data corresponds to
                        switch (i) {
                            // Type of class, not currently used
                            case 0:
                                break;
                            // Time field
                            case 1:
                                time = dataCell.getTextContent();
                                break;
                            // Days of week field
                            case 2:
                                // Initialize dayOfWeek variable
                                String dayOfWeek = dataCell.getTextContent();
                                // Switch that checks which day of the week it is
                                // Changes date advancer to match day of week
                                // e.g. Monday = no advancement, Tuesday = one day advancement
                                switch (dayOfWeek) {
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
                                room = dataCell.getTextContent();
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
                                teacherName = dataCell.getTextContent();
                                break;
                        }

                        // Increment row counter
                        i++;
                    }

                    // Create the Events title class name with room number in parentheses
                    String title = clss + "(" +  room.split(" ")[1] + ")";
                    // Create the Events description each item is on a new line
                    // Includes class name, time of class, room number, teachers name
                    String description = clss + "\n" + time + "\n" + room + "\n" + teacherName;

                    // Create a Date formatter that will be used to format a time string
                    DateFormat format = new SimpleDateFormat("MMM dd, yyyy hh:mm aa", Locale.ENGLISH);

                    // Split the time range
                    String[] splitTime = time.split("-");

                    // Add the start and end times to the date, parse it and convert it to a Date object
                    java.util.Date startDate = format.parse(weekStart + " " + splitTime[0].trim());
                    java.util.Date endDate = format.parse(weekStart + " " + splitTime[1].trim());

                    // Add the date advance to the date
                    // 86400000 milliseconds = 1 day = 24 hours
                    Date startDateTime = new Date(startDate.getTime() + advDate * 86400000);
                    Date endDateTime = new Date(endDate.getTime() + advDate * 86400000);

                    // Pass variables to add event to calendar function
                    addEventToCalendar(ical, startDateTime, endDateTime, description, title);
                }

                // Add class to list so there are no duplicate events
                clssList.add(clss);
            }

            // Choose file to write to
            File file = new File(args[2], "semester_schedule.ics");

            // Write calendar to file
            Biweekly.write(ical).go(file);
        } catch (Exception ignored) {
            // Who even reads these anymore!
            ignored.printStackTrace();
        }
    }

    // Add event to provided calendar
    // Requires event start and end dates, event description and event title/summary
    public static void addEventToCalendar(ICalendar iCal, Date startDate, Date endDate, String description, String title) {
        try {
            // Create a blank event with default settings
            VEvent event = new VEvent();

            // Set the title/summary
            event.setSummary(title);

            // Set event start and end dates
            event.setDateStart(startDate);
            event.setDateEnd(endDate);

            // Set event description
            event.setDescription(new Description(description));
            // Set event location
            event.setLocation("Durham College, Oshawa, ON, Canada");

            // Create and set UUID for event
            UUID uuid = UUID.randomUUID();
            String uuidValue = uuid.toString().replaceAll("-", "").substring(0,26) + "@durhamcollege.ca";
            event.setUid(new Uid(uuidValue));

            // Add event to calendar
            iCal.addEvent(event);
        } catch (Exception ignored) {
            // Again, who even reads these anymore!
            ignored.printStackTrace();
        }
    }
}
