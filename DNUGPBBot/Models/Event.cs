using System;
using System.Globalization;

namespace DNUGPBBot.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }

        public DateTime DateToDateTime()
        {
            return DateTime.ParseExact(this.Date.Trim(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
