using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoggingTimesTrackerDAL.Entities
{
    public class JoggingTime
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double Distance { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double Time { get; set; }

        [Required]
        public string UserId { get; set; }
        [Required]
        public ApplicationUser User { get; set; }
    }
}
