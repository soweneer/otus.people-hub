using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace PeopleHub.Model;

public record SearchUserRequest
{
    [RegularExpression("^[a-zA-Zа-яА-ЯёЁ-]*$", ErrorMessage = "Используйте только буквы и дефис в имени")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [RegularExpression("^[a-zA-Zа-яА-ЯёЁ-]*$", ErrorMessage = "Используйте только буквы и дефис в фамилии")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
}

public record SearchUserPaginatedRequest
{
    [FromQuery(Name = "first_name")]
    [Required(ErrorMessage = "Параметр first_name обязателен")]
    [RegularExpression("^[a-zA-Zа-яА-ЯёЁ-]*$", ErrorMessage = "Используйте только буквы и дефис в имени")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [FromQuery(Name = "last_name")]
    [Required(ErrorMessage = "Параметр last_name обязателен")]
    [RegularExpression("^[a-zA-Zа-яА-ЯёЁ-]*$", ErrorMessage = "Используйте только буквы и дефис в фамилии")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [FromQuery(Name = "take")]
    [Range(1, int.MaxValue, ErrorMessage = "take не может быть <= 1")]
    public int? Take { get; set; } = 50;

    [FromQuery(Name = "skip")]
    [Range(0, int.MaxValue, ErrorMessage = "skip не может быть <= 0")]
    public int? Skip { get; set; } = 0;
}
