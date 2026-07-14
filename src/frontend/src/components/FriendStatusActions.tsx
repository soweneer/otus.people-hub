import { useState } from 'react';
import { friendsApi } from '../api/client';
import { FriendRequestStatus } from '../api/types';

interface Props {
  userId: number;
  status: FriendRequestStatus;
  onStatusChange: (status: FriendRequestStatus) => void;
}

/** Кнопки/бейджи статуса дружбы — как в старых Razor-вьюхах _UserRows и User */
export function FriendStatusActions({ userId, status, onStatusChange }: Props) {
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const run = async (action: () => Promise<void>, nextStatus: FriendRequestStatus) => {
    setBusy(true);
    setError(null);
    try {
      await action();
      onStatusChange(nextStatus);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ошибка');
    } finally {
      setBusy(false);
    }
  };

  if (error) {
    return (
      <div className="alert alert-danger mb-0 py-1">
        <strong>{error}</strong>
      </div>
    );
  }

  switch (status) {
    case FriendRequestStatus.None:
      return (
        <button
          className="btn btn-success"
          title="Добавить в друзья"
          disabled={busy}
          onClick={() => void run(() => friendsApi.sendRequest(userId), FriendRequestStatus.Sent)}
        >
          <i className="fa fa-user-plus"></i>
        </button>
      );
    case FriendRequestStatus.Sent:
    case FriendRequestStatus.Rejected:
      return (
        <div className="alert alert-info mb-0 py-1">
          <i className="fa fa-clock"></i> Заявка отправлена
        </div>
      );
    case FriendRequestStatus.Approved:
      return (
        <div className="d-flex align-items-center gap-2">
          <div className="alert alert-success mb-0 py-1">
            <i className="fa fa-check"></i> У вас в друзьях
          </div>
          <button
            className="btn btn-danger"
            title="Удалить из друзей"
            disabled={busy}
            onClick={() => void run(() => friendsApi.cancel(userId), FriendRequestStatus.None)}
          >
            <i className="fa fa-user-times"></i>
          </button>
        </div>
      );
    default:
      return (
        <div className="alert alert-danger mb-0 py-1">
          <strong>Неизвестный статус заявки: {status}</strong>
        </div>
      );
  }
}
