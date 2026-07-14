import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { usersApi } from '../api/client';
import { Gender, type FriendInfo, type FriendRequestStatus } from '../api/types';
import { FriendStatusActions } from '../components/FriendStatusActions';

export function UserPage() {
  const { id } = useParams();
  const userId = Number(id);
  const [user, setUser] = useState<FriendInfo | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    usersApi
      .getById(userId)
      .then((data) => {
        if (!cancelled) setUser(data);
      })
      .catch((e) => {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Не удалось загрузить пользователя');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [userId]);

  if (loading) {
    return (
      <div className="d-flex justify-content-center mt-5">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Загрузка...</span>
        </div>
      </div>
    );
  }

  if (error || !user) {
    return <div className="alert alert-danger">{error ?? 'Пользователь не найден'}</div>;
  }

  const setStatus = (status: FriendRequestStatus) => setUser({ ...user, status });

  return (
    <div className="row">
      <div className="col-4"></div>
      <div className="col-6">
        <div className="card" style={{ width: '18rem' }}>
          <div className="card-body">
            <h5 className="card-title">
              {user.surname} {user.name}
            </h5>
            <p className="card-text">{user.bio}</p>
          </div>
          <ul className="list-group list-group-flush">
            <li className="list-group-item">Возраст: {user.age}</li>
            <li className="list-group-item">Город: {user.city}</li>
            <li className="list-group-item">Пол: {user.gender === Gender.Male ? 'Муж' : 'Жен'}</li>
          </ul>
          <div className="card-body">
            <FriendStatusActions userId={user.id} status={user.status} onStatusChange={setStatus} />
          </div>
        </div>
      </div>
    </div>
  );
}
