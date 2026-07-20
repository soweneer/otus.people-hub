using PeopleHub.Domain.Exceptions;
using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Entities;

public sealed class User(long id, PersonalInfo personalInfo)
{
    public long Id { get; } = id;
    public PersonalInfo PersonalInfo { get; private set; } = personalInfo;

    public static User Create(PersonalInfo personalInfo) =>
        new(0, Validate(personalInfo));

    public static User Restore(long id, PersonalInfo personalInfo) =>
        new(id, personalInfo);

    public void UpdatePersonalInfo(PersonalInfo personalInfo) =>
        PersonalInfo = Validate(personalInfo);

    private static PersonalInfo Validate(PersonalInfo personalInfo) =>
        string.IsNullOrWhiteSpace(personalInfo.Name) || string.IsNullOrWhiteSpace(personalInfo.Surname)
            ? throw new DomainException("Имя и фамилия пользователя не могут быть пустыми")
            : personalInfo;
}
