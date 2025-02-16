using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppsTracker.Data.Models
{
    [Table("Recap")]
    public class Recap
    {
        [Key]
        public int RecapID { get; set; }
        public DateTime Timestamp { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int ApplicationID { get; set; }
        public string ApplicationName { get; set; }
        public int WindowID { get; set; }
        public string WindowTitle { get; set; }
        public long Duration { get; set; }
    }
}

