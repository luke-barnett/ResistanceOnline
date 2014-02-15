using System.ComponentModel.DataAnnotations;

namespace ResistanceOnline.Site.Models
{
	public class LoginConfirmationViewModel
	{
		[Required]
		[Display(Name = "User name")]
		public string UserName { get; set; }
	}
}