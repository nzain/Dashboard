using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using nZain.Dashboard.Models;

namespace nZain.Dashboard.Services
{
    public class GoogleCalendarService
    {
        private const string CredentialsFile = "Secrets/credentials.json";

        private const string TokenPath = "Secrets/token.json";

        private const string ApplicationName = "nzain-dashboard";

        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/calendar-dotnet-quickstart.json
        private static readonly string[] Scopes = { CalendarService.Scope.CalendarReadonly };

        #region Static Authentication

        public static async Task<CalendarService> GoogleCalendarAuthAsync()
        {
            UserCredential credential;
            try
            {
                using (var stream = new FileStream(CredentialsFile, FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(TokenPath, true));
                    Console.WriteLine("Credential file saved to: " + TokenPath);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Failed to authenticate");
                Console.Error.WriteLine(e);
                throw;
            }

            // Create Google Calendar API service.
            return new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }
        
        #endregion

        #region Service

        private readonly CalendarService _googleService;
        private readonly CalendarListService _calendarListService;

        public GoogleCalendarService(CalendarService googleService, CalendarListService calendarListService)
        {
            this._googleService = googleService ?? throw new ArgumentNullException(nameof(googleService));
            this._calendarListService = calendarListService ?? throw new ArgumentNullException(nameof(calendarListService));

            CalendarList calendars = this._googleService.CalendarList.List().Execute();
            foreach (CalendarListEntry cal in calendars.Items)
            {
                System.Console.WriteLine($"Calendar {cal.Id}: {cal.Summary}");
            }
        }

        public IEnumerable<CalendarDay> EnumerateDays(int n)
        {
            DateTimeOffset d = DateTimeOffset.Now;
            for (int i = 0; i < n; i++)
            {
                CalendarDay item = new CalendarDay(d, new CalendarEvent[0]);
                yield return item;
                d = d.AddDays(+1);
            }
        }

        public async Task<CalendarDay[]> GetDataAsync(int n)
        {
            if (n < 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            DateTimeOffset d = DateTimeOffset.Now;
            d = new DateTimeOffset(d.Year, d.Month, d.Day, 0, 0, 0, d.Offset);
            List<CalendarDay> results = new List<CalendarDay>();

            // TODO create days first, properly set start/end
            // TODO then populate with events from each calendar response

            foreach (string calendarId in this._calendarListService.CalendarIds)
            {
                Events response = await this.QueryCalendarEventsAsync(calendarId, n, d);
                results.AddRange(EnumerateDays(n, response, d));
            }
            
            // TODO MERGE days for multiple calendars

            return results.ToArray();
        }

        private Task<Events> QueryCalendarEventsAsync(string calendarId, int n, DateTimeOffset d)
        {
            // Define parameters of request.
            EventsResource.ListRequest request = this._googleService.Events.List(calendarId);
            request.TimeMin = d.Date;
            request.TimeMax = new DateTimeOffset(d.Year, d.Month, d.Day + n, 23, 59, 59, d.Offset).Date;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 20;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // async WebRequest
            return request.ExecuteAsync();
        }

        private static IEnumerable<CalendarDay> EnumerateDays(int n, Events response, DateTimeOffset d)
        {
            CalendarEvent[] items = response.Items // multiple enumerations... might give unexpected result?
                .Select(s => new CalendarEvent(s))
                .ToArray();

            for (int i = 0; i < n; i++)
            {
                // for each day, filter the results accordingly
                CalendarEvent[] events = items.Where(w => w.IsActiveAt(d)).ToArray();
                CalendarDay item = new CalendarDay(d, events);
                yield return item;
                d = d.AddDays(+1);
            }
        }

        #endregion
    }
}