using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace waterprj.Models
{
    public class Consumption
    {
        public int Id { get; set; }
        public string UserId { get; set; }
      
        [Required(ErrorMessage = "The Date field is required.")]
        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "The Volume field is required.")]
        [Display(Name = "Volume")]
        public double Volume { get; set; }
    }

    public static class ClaimsPrincipalExtensions
    {
        public static string? getUserId(this ClaimsPrincipal user)
        {
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return null;
            }

            var currentUser = user;
            var nameIdentifierClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier);
            if (nameIdentifierClaim == null)
            {
                return null;
            }

            return nameIdentifierClaim.Value;
        }
    }
}
