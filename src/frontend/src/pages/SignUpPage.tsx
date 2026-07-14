import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { EMAIL_REGEX, validatePersonFields, type FieldErrors } from '../validation';

interface FormValues {
  name: string;
  surname: string;
  gender: string;
  age: string;
  city: string;
  bio: string;
  email: string;
  password: string;
  confirmPassword: string;
}

const initialValues: FormValues = {
  name: '',
  surname: '',
  gender: '',
  age: '',
  city: '',
  bio: '',
  email: '',
  password: '',
  confirmPassword: '',
};

function validate(values: FormValues): FieldErrors<keyof FormValues> {
  const errors: FieldErrors<keyof FormValues> = { ...validatePersonFields(values) };

  if (!values.email.trim()) errors.email = 'Не указан email';
  else if (!EMAIL_REGEX.test(values.email)) errors.email = 'Некорректный email';

  if (!values.password) errors.password = 'Не указан пароль';
  if (values.confirmPassword !== values.password) errors.confirmPassword = 'Пароли не совпадают';

  return errors;
}

export function SignUpPage() {
  const { signUp } = useAuth();
  const navigate = useNavigate();
  const [values, setValues] = useState<FormValues>(initialValues);
  const [errors, setErrors] = useState<FieldErrors<keyof FormValues>>({});
  const [serverError, setServerError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const set = (field: keyof FormValues) => (value: string) =>
    setValues((prev) => ({ ...prev, [field]: value }));

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const nextErrors = validate(values);
    setErrors(nextErrors);
    setServerError(null);
    if (Object.keys(nextErrors).length > 0) return;

    setBusy(true);
    try {
      await signUp({
        name: values.name,
        surname: values.surname,
        city: values.city,
        email: values.email,
        password: values.password,
        confirmPassword: values.confirmPassword,
        bio: values.bio,
        age: Number(values.age),
        gender: Number(values.gender),
      });
      navigate('/people');
    } catch (err) {
      setServerError(err instanceof Error ? err.message : 'Не удалось зарегистрировать пользователя');
    } finally {
      setBusy(false);
    }
  };

  const textField = (
    field: keyof FormValues,
    label: string,
    type = 'text',
  ) => (
    <div className="form-group">
      <label htmlFor={field}>{label}</label>
      <input
        id={field}
        type={type}
        className={`form-control ${errors[field] ? 'is-invalid' : ''}`}
        value={values[field]}
        onChange={(e) => set(field)(e.target.value)}
      />
      {errors[field] && <div className="invalid-feedback">{errors[field]}</div>}
    </div>
  );

  return (
    <div className="row">
      <div className="col-2"></div>
      <div className="col-8">
        <h2>Регистрация</h2>
        {serverError && <div className="alert alert-danger">{serverError}</div>}
        <form onSubmit={(e) => void handleSubmit(e)} noValidate>
          {textField('name', 'Имя')}
          {textField('surname', 'Фамилия')}
          <div className="form-group">
            <label>Пол</label>
            <div className="form-check">
              <input
                className="form-check-input"
                type="radio"
                name="gender"
                value="1"
                id="male"
                checked={values.gender === '1'}
                onChange={(e) => set('gender')(e.target.value)}
              />
              <label className="form-check-label" htmlFor="male">
                Парень
              </label>
            </div>
            <div className="form-check">
              <input
                className="form-check-input"
                type="radio"
                name="gender"
                value="0"
                id="female"
                checked={values.gender === '0'}
                onChange={(e) => set('gender')(e.target.value)}
              />
              <label className="form-check-label" htmlFor="female">
                Девушка
              </label>
            </div>
            {errors.gender && <div className="text-danger small">{errors.gender}</div>}
          </div>
          {textField('age', 'Ваш возраст', 'number')}
          {textField('city', 'Город')}
          <div className="form-group">
            <label htmlFor="bio">Пару слов о себе</label>
            <textarea
              id="bio"
              className={`form-control ${errors.bio ? 'is-invalid' : ''}`}
              value={values.bio}
              onChange={(e) => set('bio')(e.target.value)}
            />
            {errors.bio && <div className="invalid-feedback">{errors.bio}</div>}
          </div>
          {textField('email', 'Email', 'email')}
          {textField('password', 'Введите пароль', 'password')}
          {textField('confirmPassword', 'Повторите пароль', 'password')}
          <div className="form-group">
            <button type="submit" className="btn btn-primary" disabled={busy}>
              Зарегистрироваться
            </button>
          </div>
        </form>
      </div>
      <div className="col-2"></div>
    </div>
  );
}
