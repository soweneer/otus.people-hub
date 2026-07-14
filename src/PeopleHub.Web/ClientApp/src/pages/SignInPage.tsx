import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export function SignInPage() {
  const { signIn } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState<{ email?: string; password?: string }>({});
  const [serverError, setServerError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const nextErrors: typeof errors = {};
    if (!email.trim()) nextErrors.email = 'Не указан email';
    if (!password) nextErrors.password = 'Не указан пароль';
    setErrors(nextErrors);
    setServerError(null);
    if (Object.keys(nextErrors).length > 0) return;

    setBusy(true);
    try {
      await signIn(email, password);
      navigate('/people');
    } catch (err) {
      setServerError(err instanceof Error ? err.message : 'Неверные данные пользователя');
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="row">
      <div className="col-2"></div>
      <div className="col-8">
        <h2>Войти</h2>
        {serverError && <div className="alert alert-danger">{serverError}</div>}
        <form onSubmit={(e) => void handleSubmit(e)} noValidate>
          <div className="form-group">
            <label htmlFor="email">Введите Email</label>
            <input
              id="email"
              type="text"
              className={`form-control ${errors.email ? 'is-invalid' : ''}`}
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
            {errors.email && <div className="invalid-feedback">{errors.email}</div>}
          </div>
          <div className="form-group">
            <label htmlFor="password">Введите пароль</label>
            <input
              id="password"
              type="password"
              className={`form-control ${errors.password ? 'is-invalid' : ''}`}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
            {errors.password && <div className="invalid-feedback">{errors.password}</div>}
          </div>
          <div className="form-group">
            <button type="submit" className="btn btn-primary" disabled={busy}>
              Войти
            </button>
          </div>
        </form>
      </div>
      <div className="col-2"></div>
    </div>
  );
}
