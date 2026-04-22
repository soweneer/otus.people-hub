using System.ComponentModel.DataAnnotations;

namespace PeopleHub.Model;

public record SearchPersonRequest
{
    [RegularExpression("^[a-zA-Zа-яА-ЯёЁ-]*$", ErrorMessage = "Используйте только буквы и дефис в имени")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [RegularExpression("^[a-zA-Zа-яА-ЯёЁ-]*$", ErrorMessage = "Используйте только буквы и дефис в фамилии")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
}

public record SearchPersonPaginatedRequest : SearchPersonRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "take не может быть <= 1")]
    public int? Take { get; set; } = 50;
    
    [Range(0, int.MaxValue, ErrorMessage = "skip не может быть <= 0")]
    public int? Skip { get; set; } = 0;
}