using System.ComponentModel.DataAnnotations;

namespace PeopleHub.Model;

public sealed record SearchPersonRequest
{
    [RegularExpression("^[a-zA-Zа-яА-ЯёЁ-]*$", ErrorMessage = "Используйте только буквы и дефис в имени")]
    [StringLength(100)]
    public string FirstName { get; set; }

    [RegularExpression("^[a-zA-Zа-яА-ЯёЁ-]*$", ErrorMessage = "Используйте только буквы и дефис в фамилии")]
    [StringLength(100)]
    public string LastName { get; set; }
}
