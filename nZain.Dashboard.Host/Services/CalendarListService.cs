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
    public class CalendarListService
    {
        // File format: "CalendarId;YourNameForIt"
        // Want to know, which calendarIds you have? Use the static method QueryCalendarIdsAsync below!
        private const string FileName = "GoogleAuth/calendars.csv";

        #region Static Factory and Helper Method

        /// <summary>Query available calendar IDs.</summary>
        /// <param name="service">The service.</param>
        /// <returns>Map of calendarIds and their summery(name).</returns>
        public static async Task<IReadOnlyDictionary<string, string>> QueryCalendarIdsAsync(CalendarService service)
        {
            var response = await service.CalendarList.List().ExecuteAsync();
            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach (var item in response.Items)
            {
                map.Add(item.Id, item.Summary);
            }
            return map;
        }

        public static async Task<CalendarListService> LoadAsync()
        {
            if (!File.Exists(FileName))
            {
                throw new FileNotFoundException(FileName);
            }
            Dictionary<string, string> idNameMap = new Dictionary<string, string>();
            using (var r = new StreamReader(FileName))
            {
                string line;
                while ((line = await r.ReadLineAsync()) != null)
                {
                    string[] columns = line.Split(';');
                    if (columns.Length != 2)
                    {
                        throw new InvalidDataException($"Failed to parse '{line}' - expected 'CalendarID;CalendarName'");
                    }
                    idNameMap.Add(columns[0], columns[1]);
                }
            }
            return new CalendarListService(idNameMap);
        }

        #endregion

        #region Class

        private CalendarListService(Dictionary<string, string> idNameMap)
        {
            this.Names = idNameMap ?? throw new ArgumentNullException();
            if (idNameMap.Count == 0)
            {
                throw new ArgumentException("no ids given?");
            }
            this.CalendarIds = idNameMap.Keys.ToArray();
        }

        public IReadOnlyDictionary<string, string> Names { get; }

        public string[] CalendarIds { get; }

        #endregion
    }
}