import { useCallback, useEffect, useState, type ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { feedApi, friendsApi } from '../api/client';
import type { FeedPost, FriendInfoLite, FriendsInfo } from '../api/types';

const FEED_LIMIT = 20;

type TabKey = 'friends' | 'feed' | 'incoming' | 'outgoing';

export function FriendsPage() {
  const [info, setInfo] = useState<FriendsInfo | null>(null);
  const [feed, setFeed] = useState<FeedPost[]>([]);
  const [activeTab, setActiveTab] = useState<TabKey>('friends');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const reload = useCallback(async () => {
    setError(null);
    try {
      const [friendsInfo, feedPosts] = await Promise.all([friendsApi.get(), feedApi.get(0, FEED_LIMIT)]);
      setInfo(friendsInfo);
      setFeed(feedPosts);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Не удалось загрузить данные');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void reload();
  }, [reload]);

  const act = async (action: () => Promise<void>) => {
    try {
      await action();
      await reload();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ошибка');
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

  if (!info) {
    return <div className="alert alert-danger">{error ?? 'Не удалось загрузить данные'}</div>;
  }

  // Если активная вкладка опустела (одобрили/отклонили последнюю заявку) — возвращаемся к друзьям
  const effectiveTab: TabKey =
    (activeTab === 'incoming' && info.incoming.length === 0) ||
    (activeTab === 'outgoing' && info.outgoing.length === 0)
      ? 'friends'
      : activeTab;

  const tab = (key: TabKey, label: string, count?: number) => (
    <li className="nav-item">
      <a
        href="#"
        className={`nav-link ${effectiveTab === key ? 'active' : ''}`}
        onClick={(e) => {
          e.preventDefault();
          setActiveTab(key);
        }}
      >
        {label} {count !== undefined && <span className="badge rounded-pill text-bg-success">{count}</span>}
      </a>
    </li>
  );

  const friendsTable = (items: FriendInfoLite[], actions: (item: FriendInfoLite) => ReactNode) => (
    <table className="table table-striped table-bordered table-hover">
      <thead className="thead-light">
        <tr>
          <th scope="col">Фамилия, имя</th>
          <th scope="col">Возраст</th>
          <th scope="col">Город</th>
          <th scope="col">#</th>
        </tr>
      </thead>
      <tbody>
        {items.map((item) => (
          <tr key={item.user.id}>
            <td>
              <Link to={`/users/${item.user.id}`}>{item.user.name}</Link>
            </td>
            <td>{item.user.age}</td>
            <td>{item.user.city}</td>
            <td>{actions(item)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );

  const authorNames = new Map(info.friends.map((f) => [f.user.id, f.user.name]));

  return (
    <>
      {error && <div className="alert alert-danger">{error}</div>}
      <ul className="nav nav-tabs" role="tablist">
        {tab('friends', 'Друзья')}
        {tab('feed', 'Лента')}
        {info.incoming.length > 0 && tab('incoming', 'Входящие', info.incoming.length)}
        {info.outgoing.length > 0 && tab('outgoing', 'Исходящие', info.outgoing.length)}
      </ul>
      <div className="tab-content">
        {effectiveTab === 'friends' && (
          <div className="pt-3">
            {info.friends.length > 0 ? (
              friendsTable(info.friends, (item) => (
                <button
                  className="btn btn-danger"
                  title="Удалить из друзей"
                  onClick={() => void act(() => friendsApi.cancel(item.user.id))}
                >
                  <i className="fa fa-user-times"></i>
                </button>
              ))
            ) : (
              <div className="alert alert-info">У вас пока нет друзей</div>
            )}
          </div>
        )}
        {effectiveTab === 'feed' && (
          <div>
            {feed.length > 0 ? (
              feed.map((post) => (
                <div className="card mt-3" key={post.id}>
                  <div className="card-header">
                    {authorNames.has(post.authorUserId) ? (
                      <Link to={`/users/${post.authorUserId}`}>{authorNames.get(post.authorUserId)}</Link>
                    ) : (
                      <span>Пользователь #{post.authorUserId}</span>
                    )}
                  </div>
                  <div className="card-body">
                    <p className="card-text">{post.text}</p>
                  </div>
                </div>
              ))
            ) : (
              <div className="alert alert-info mt-3">В ленте пока нет постов</div>
            )}
          </div>
        )}
        {effectiveTab === 'incoming' && (
          <div className="pt-3">
            {friendsTable(info.incoming, (item) => (
              <>
                <button
                  className="btn btn-success me-1"
                  title="Добавить в друзья"
                  onClick={() => void act(() => friendsApi.approve(item.friendRequestId))}
                >
                  <i className="fa fa-user-plus"></i>
                </button>
                <button
                  className="btn btn-secondary"
                  title="Оставить в подписчиках"
                  onClick={() => void act(() => friendsApi.reject(item.friendRequestId))}
                >
                  <i className="fa fa-ban"></i>
                </button>
              </>
            ))}
          </div>
        )}
        {effectiveTab === 'outgoing' && (
          <div className="pt-3">
            {friendsTable(info.outgoing, (item) => (
              <button
                className="btn btn-warning"
                title="Отменить заявку"
                onClick={() => void act(() => friendsApi.cancel(item.user.id))}
              >
                <i className="fa fa-ban"></i>
              </button>
            ))}
          </div>
        )}
      </div>
    </>
  );
}
