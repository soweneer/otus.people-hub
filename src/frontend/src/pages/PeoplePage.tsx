import { useCallback, useEffect, useRef, useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { usersApi } from '../api/client';
import type { FriendRequestStatus, UserInfo } from '../api/types';
import { FriendStatusActions } from '../components/FriendStatusActions';
import { SEARCH_REGEX } from '../validation';

const PAGE_SIZE = 20;

export function PeoplePage() {
  const [firstNameInput, setFirstNameInput] = useState('');
  const [lastNameInput, setLastNameInput] = useState('');
  // Применённый фильтр — по нему идёт загрузка
  const [filter, setFilter] = useState({ firstName: '', lastName: '' });
  const [rows, setRows] = useState<UserInfo[]>([]);
  const [hasMore, setHasMore] = useState(true);
  const [loading, setLoading] = useState(false);
  const [searchError, setSearchError] = useState<string | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);

  const sentinelRef = useRef<HTMLDivElement | null>(null);
  const inFlightRef = useRef(false);
  // Номер «эпохи» поиска: ответы устаревших эпох отбрасываются
  const epochRef = useRef(0);
  const stateRef = useRef({ rows, hasMore, filter });
  stateRef.current = { rows, hasMore, filter };

  const loadNext = useCallback(async () => {
    if (inFlightRef.current) return;
    const { rows, hasMore, filter } = stateRef.current;
    if (!hasMore) return;
    const epoch = epochRef.current;
    inFlightRef.current = true;
    setLoading(true);
    try {
      const page = await usersApi.search(filter.firstName, filter.lastName, rows.length, PAGE_SIZE);
      if (epoch !== epochRef.current) return;
      setRows((prev) => [...prev, ...page]);
      if (page.length < PAGE_SIZE) setHasMore(false);
    } catch (e) {
      if (epoch === epochRef.current) {
        setLoadError(e instanceof Error ? e.message : 'Не удалось загрузить пользователей');
        setHasMore(false);
      }
    } finally {
      inFlightRef.current = false;
      setLoading(false);
    }
  }, []);

  // Первая страница — при монтировании и смене фильтра
  useEffect(() => {
    void loadNext();
  }, [filter, loadNext]);

  // Догружаем, пока сентинел в пределах видимости (короткая страница)
  useEffect(() => {
    if (loading || !hasMore || searchError) return;
    const sentinel = sentinelRef.current;
    if (sentinel && sentinel.getBoundingClientRect().top < window.innerHeight + 200) {
      void loadNext();
    }
  }, [rows, loading, hasMore, searchError, loadNext]);

  // Догрузка по скроллу
  useEffect(() => {
    const sentinel = sentinelRef.current;
    if (!sentinel || searchError) return;
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting) void loadNext();
      },
      { rootMargin: '200px 0px' },
    );
    observer.observe(sentinel);
    return () => observer.disconnect();
  }, [loadNext, searchError]);

  const handleSearch = (e: FormEvent) => {
    e.preventDefault();
    const firstName = firstNameInput.trim();
    const lastName = lastNameInput.trim();

    if (!SEARCH_REGEX.test(firstName)) {
      setSearchError('Используйте только буквы и дефис в имени');
      return;
    }
    if (!SEARCH_REGEX.test(lastName)) {
      setSearchError('Используйте только буквы и дефис в фамилии');
      return;
    }
    if (!firstName && !lastName) {
      setSearchError('Заполните хотя бы одно из полей: имя или фамилия');
      return;
    }

    setSearchError(null);
    setLoadError(null);
    epochRef.current += 1;
    setRows([]);
    setHasMore(true);
    setFilter({ firstName, lastName });
  };

  const setRowStatus = (userId: number) => (status: FriendRequestStatus) =>
    setRows((prev) => prev.map((r) => (r.user.id === userId ? { ...r, status } : r)));

  const showNoData = !loading && !searchError && !loadError && rows.length === 0 && !hasMore;

  return (
    <>
      <form onSubmit={handleSearch} className="mb-3">
        <div className="row g-2 align-items-start">
          <div className="col">
            <input
              type="text"
              className="form-control"
              placeholder="Имя"
              value={firstNameInput}
              onChange={(e) => setFirstNameInput(e.target.value)}
            />
          </div>
          <div className="col">
            <input
              type="text"
              className="form-control"
              placeholder="Фамилия"
              value={lastNameInput}
              onChange={(e) => setLastNameInput(e.target.value)}
            />
          </div>
          <div className="col-auto">
            <button type="submit" className="btn btn-primary">
              Search
            </button>
          </div>
        </div>
      </form>

      {searchError && <div className="alert alert-warning">{searchError}</div>}
      {loadError && <div className="alert alert-danger">{loadError}</div>}
      {showNoData && <div className="alert alert-info">Нет данных по людям</div>}

      {!searchError && rows.length > 0 && (
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
            {rows.map((row) => (
              <tr key={row.user.id}>
                <td>
                  <Link to={`/users/${row.user.id}`}>{row.user.name}</Link>
                </td>
                <td>{row.user.age}</td>
                <td>{row.user.city}</td>
                <td>
                  <FriendStatusActions
                    userId={row.user.id}
                    status={row.status}
                    onStatusChange={setRowStatus(row.user.id)}
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {loading && (
        <div className="loading-overlay">
          <div className="spinner-border text-light" role="status" style={{ width: '10rem', height: '10rem' }}>
            <span className="visually-hidden">Загрузка...</span>
          </div>
        </div>
      )}

      {!searchError && <div ref={sentinelRef} style={{ height: 1 }}></div>}
    </>
  );
}
