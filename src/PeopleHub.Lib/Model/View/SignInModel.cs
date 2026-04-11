using System.ComponentModel.DataAnnotations;

namespace PeopleHub.Lib.Model.View
{
    public sealed class SignInModel
    {
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage ="Не указан email")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}