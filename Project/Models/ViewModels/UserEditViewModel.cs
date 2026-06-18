using System.ComponentModel.DataAnnotations;

namespace RaceReader.Models.ViewModels
{
    public class UserEditViewModel
    {
        public string Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Token Balance")]
        public int TokenBalance { get; set; }
        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }
    }
}
