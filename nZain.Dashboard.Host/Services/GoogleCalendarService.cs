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
using nZain.Dashboard.Host;
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

        private readonly string[] _calendarIds;
        private readonly CalendarService _googleService;

        public GoogleCalendarService(DashboardConfig cfg, CalendarService googleService)
        {
            Dictionary<string, string> calendars = cfg?.Calendars;
            if (calendars == null)
            {
                throw new InvalidDataException("DashboardConfig.Calendars is null");
            }
            if (calendars.Count == 0)
            {
                throw new InvalidDataException("DashboardConfig.Calendars is empty");
            }
            this._calendarIds = calendars.Keys.ToArray();
            this._googleService = googleService ?? throw new ArgumentNullException(nameof(googleService));
        }

        public async Task<CalendarDay[]> GetCalendarEventsAsync(int n)
        {
            if (n < 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            DateTimeOffset d = DateTimeOffset.Now;
            d = new DateTimeOffset(d.Year, d.Month, d.Day, 0, 0, 0, d.Offset);

            // TODO create days first, properly set start/end
            // TODO then populate with events from each calendar response

            List<Events> responses = new List<Events>();
            foreach (string calendarId in this._calendarIds)
            {
                Events response = await this.QueryCalendarEventsAsync(calendarId, n, d);
                responses.Add(response);
            }

            // Merge results of multiple calendars
            return EnumerateDays(n, responses, d).ToArray();
        }

        private Task<Events> QueryCalendarEventsAsync(string calendarId, int n, DateTimeOffset d)
        {
            // Define parameters of request.
            EventsResource.ListRequest request = this._googleService.Events.List(calendarId);
            request.TimeMin = d.Date;
            request.TimeMax = new DateTimeOffset(d.Year, d.Month, d.Day, 23, 59, 59, d.Offset).AddDays(n).Date;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 20;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // async WebRequest
            return request.ExecuteAsync();
        }

        private static IEnumerable<CalendarDay> EnumerateDays(int n, List<Events> responses, DateTimeOffset d)
        {
            // avoid multiple enumerations... might give unexpected result?
            CalendarEvent[] items = responses
                .SelectMany(s => s.Items)
                .Select(s => new CalendarEvent(s))
                .ToArray();

            for (int i = 0; i < n; i++)
            {
                // for each day, filter the results of all calendars accordingly
                CalendarEvent[] events = items.Where(w => w.IsActiveAt(d)).ToArray();
                CalendarDay item = new CalendarDay(d, events);
                yield return item;
                d = d.AddDays(+1);
            }
        }

        #endregion
    }
}