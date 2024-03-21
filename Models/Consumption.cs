namespace waterprj.Models
{
    public class Consumption
    {
       
        
            public int Id { get; set; }
            public int UserId { get; set; }
            public DateTime Date { get; set; }
            public double Volume { get; set; } // Consumption volume in liters



        
    }
}
