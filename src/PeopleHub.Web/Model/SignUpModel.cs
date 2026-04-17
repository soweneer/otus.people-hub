using System.ComponentModel.DataAnnotations;

namespace PeopleHub.Model
{
    public sealed class SignUpModel
    {
        [Required(ErrorMessage = "Не указано имя")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я-]+$", ErrorMessage = "Используйте только буквы и дефис")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Не указана фамилия")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я-]+$", ErrorMessage = "Используйте только буквы и дефис")]
        [StringLength(100)]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Не указан город")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я\s-]+$", ErrorMessage = "Используйте только буквы, пробел или дефис")]
        [StringLength(100)]
        public string City { get; set; }

        [Required(ErrorMessage = "Не указан email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }

        [RegularExpression(@"^[a-zA-Zа-яА-Я,.\s-]+$", ErrorMessage = "Используйте только буквы, пробел или дефис")]
        [StringLength(400, ErrorMessage = "Слишком много слов")]
        public string Bio { get; set; }

        [Required(ErrorMessage = "Укажите возраст")]
        [Range(18, 100, ErrorMessage = "Некорректный возраст для земного человека 21 века")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Укажите пол")]
        public int Gender { get; set; }
    }
}
