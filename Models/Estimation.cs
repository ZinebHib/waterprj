using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace waterprj.Models
{
    public class Estimation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int Id { get; set; } // Primary Key
       
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public double EstimatedVolume { get; set; } // Estimated consumption volume in liters

        // User-provided attributes for simple estimation
        [Required(ErrorMessage = "Please enter the number of people!")]
        public int NumberOfPeople { get; set; }

        public bool HasPool { get; set; }
        public bool UsesDishwasher { get; set; }
        [Required(ErrorMessage = "Please enter the laundry frequency !")]
        public int LaundryFrequency { get; set; } // (e.g., daily, weekly)
        [Required(ErrorMessage = "Please enter Shower duration !")]
        public int ShowerDuration { get; set; } // (e.g., minutes per shower)
        public bool LeakDetection { get; set; } // (has leak detection system)
    }
}
