// Правила повторяют DataAnnotations серверных моделей (SignUpRequest, UpdateMyProfileRequest, SearchUserRequest)
export const NAME_REGEX = /^[a-zA-Zа-яА-Я-]+$/;
export const CITY_REGEX = /^[a-zA-Zа-яА-Я\s-]+$/;
export const BIO_REGEX = /^[a-zA-Zа-яА-Я,.\s-]+$/;
export const SEARCH_REGEX = /^[a-zA-Zа-яА-ЯёЁ-]*$/;
export const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export type FieldErrors<T extends string> = Partial<Record<T, string>>;

export interface PersonFields {
  name: string;
  surname: string;
  age: string;
  city: string;
  bio: string;
  gender: string;
}

export function validatePersonFields(values: PersonFields): FieldErrors<keyof PersonFields> {
  const errors: FieldErrors<keyof PersonFields> = {};

  if (!values.name.trim()) errors.name = 'Не указано имя';
  else if (!NAME_REGEX.test(values.name)) errors.name = 'Используйте только буквы и дефис';
  else if (values.name.length > 100) errors.name = 'Слишком длинное имя';

  if (!values.surname.trim()) errors.surname = 'Не указана фамилия';
  else if (!NAME_REGEX.test(values.surname)) errors.surname = 'Используйте только буквы и дефис';
  else if (values.surname.length > 100) errors.surname = 'Слишком длинная фамилия';

  if (!values.age.trim()) errors.age = 'Укажите возраст';
  else {
    const age = Number(values.age);
    if (!Number.isInteger(age) || age < 18 || age > 100)
      errors.age = 'Некорректный возраст для земного человека 21 века';
  }

  if (!values.city.trim()) errors.city = 'Не указан город';
  else if (!CITY_REGEX.test(values.city)) errors.city = 'Используйте только буквы, пробел или дефис';
  else if (values.city.length > 100) errors.city = 'Слишком длинное название города';

  if (values.bio) {
    if (values.bio.length > 400) errors.bio = 'Слишком много слов';
    else if (!BIO_REGEX.test(values.bio)) errors.bio = 'Используйте только буквы, пробел или дефис';
  }

  if (values.gender !== '0' && values.gender !== '1') errors.gender = 'Укажите пол';

  return errors;
}
