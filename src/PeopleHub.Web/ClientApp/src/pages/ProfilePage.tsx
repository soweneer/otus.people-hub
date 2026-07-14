import { useEffect, useState, type FormEvent } from 'react';
import { profileApi } from '../api/client';
import { validatePersonFields, type FieldErrors, type PersonFields } from '../validation';

const emptyForm: PersonFields = { name: '', surname: '', age: '', city: '', bio: '', gender: '' };

export function ProfilePage() {
  const [values, setValues] = useState<PersonFields>(emptyForm);
  const [errors, setErrors] = useState<FieldErrors<keyof PersonFields>>({});
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [saved, setSaved] = useState(false);
  const [serverError, setServerError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    profileApi
      .get()
      .then((profile) => {
        if (cancelled) return;
        setValues({
          name: profile.name ?? '',
          surname: profile.surname ?? '',
          age: String(profile.age ?? ''),
          city: profile.city ?? '',
          bio: profile.bio ?? '',
          gender: String(profile.gender ?? ''),
        });
      })
      .catch((e) => {
        if (!cancelled) setServerError(e instanceof Error ? e.message : 'Не удалось загрузить профиль');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const set = (field: keyof PersonFields) => (value: string) =>
    setValues((prev) => ({ ...prev, [field]: value }));

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const nextErrors = validatePersonFields(values);
    setErrors(nextErrors);
    setSaved(false);
    setServerError(null);
    if (Object.keys(nextErrors).length > 0) return;

    setBusy(true);
    try {
      await profileApi.update({
        name: values.name,
        surname: values.surname,
        age: Number(values.age),
        city: values.city,
        bio: values.bio,
        gender: Number(values.gender),
      });
      setSaved(true);
    } catch (err) {
      setServerError(err instanceof Error ? err.message : 'Не удалось сохранить профиль');
    } finally {
      setBusy(false);
    }
  };

  if (loading) {
    return (
      <div className="d-flex justify-content-center mt-5">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Загрузка...</span>
        </div>
      </div>
    );
  }

  const textField = (field: keyof PersonFields, label: string, type = 'text') => (
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
        <h2>Профиль пользователя</h2>
        {saved && (
          <div className="alert alert-success">
            <i className="fa fa-check"></i> Данные профиля успешно сохранены
          </div>
        )}
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
          <div className="form-group">
            <button type="submit" className="btn btn-primary" disabled={busy}>
              Сохранить
            </button>
          </div>
        </form>
      </div>
      <div className="col-2"></div>
    </div>
  );
}
