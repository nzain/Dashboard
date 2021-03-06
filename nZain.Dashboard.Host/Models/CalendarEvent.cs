
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Google.Apis.Calendar.v3.Data;

namespace nZain.Dashboard.Models
{
    public class CalendarEvent
    {
        private static readonly IReadOnlyDictionary<string, string> Prefixes = new Dictionary<string, string>
        {
            // ICalUID                                                                 Prefix
            { "cos68chpcgsj6bb1c8qj2b9kcph64b9p6kr6ab9lccoj0eb26cqj6d1l6g@google.com", "S: " }
        }.ToImmutableDictionary();
        
        public CalendarEvent(Event ev)
        {
            this.StartTime = Convert(ev.Start, out bool allDay);
            this.EndTime = allDay
                ? Convert(ev.End, out _).Subtract(TimeSpan.FromSeconds(1))
                : Convert(ev.End, out _);
            this.IsAllDay = allDay;
            this.IsMultiDay = this.StartTime.Day != this.EndTime.Day;
            if (Prefixes.TryGetValue(ev.ICalUID, out string prefix) && !ev.Summary.StartsWith(prefix))
            {
                this.Summary = prefix + ev.Summary;
            }
            else
            {
                this.Summary = ev.Summary;
            }
            this.Location = ev.Location;
            this.DisplayTime = allDay? null : this.StartTime.ToString("HH:mm");
        }

        private static DateTimeOffset Convert(EventDateTime edt, out bool allDay)
        {
            if (edt == null) 
            {
                allDay = true;
                return default(DateTimeOffset);
            }
            if (edt.DateTime.HasValue)
            {
                allDay = false;
                return new DateTimeOffset(edt.DateTime.Value);
            }
            // all day event 'yyyy-MM-dd'
            if (DateTimeOffset.TryParseExact(edt.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTimeOffset result))
            {
                allDay = true;
                return result;
            }
            allDay = true;
            return default(DateTimeOffset);
        }

        public DateTimeOffset StartTime { get; }

        public DateTimeOffset EndTime { get; }
        
        public bool IsAllDay { get; }

        public bool IsMultiDay { get; }

        public string DisplayTime { get; }

        public string Summary { get; }

        public string Location { get; }

        public bool IsActiveAt(DateTimeOffset d)
        {
            if (this.IsAllDay)
            {
                return this.StartTime <= d && d <= this.EndTime;
            }
            var s = this.StartTime;
            s = new DateTimeOffset(s.Year, s.Month, s.Day, 0, 0, 0, s.Offset);
            var e = this.EndTime;
            e = new DateTimeOffset(e.Year, e.Month, e.Day, 23, 59, 59, s.Offset);
            return s <= d && d <= e;
        }

        public override string ToString()
        {
            if (this.IsAllDay)
            {
                return this.Summary;
            }
            return $"{this.StartTime.ToString("HH:mm")}-{this.EndTime.ToString("HH:mm")} {this.Summary}";
        }
    }
}