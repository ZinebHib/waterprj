namespace waterprj.Models
{
    public class Estimation
    {
        public int Id { get; set; } // Primary Key
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public double EstimatedVolume { get; set; } // Estimated consumption volume in liters

        // User-provided attributes for simple estimation
        public int NumberOfPeople { get; set; }
        public bool HasPool { get; set; }
        public bool UsesDishwasher { get; set; }
        public int LaundryFrequency { get; set; } // (e.g., daily, weekly)
        public int ShowerDuration { get; set; } // (e.g., minutes per shower)
        public bool LeakDetection { get; set; } // (has leak detection system)
    }
}
