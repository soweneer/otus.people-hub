import { useCallback, useEffect, useRef, useState, type FormEvent } from 'react';
import { dialogApi, friendsApi } from '../api/client';
import type { DialogMessage, DialogPartner, FriendInfoLite } from '../api/types';

export function DialogsPage() {
  const [partners, setPartners] = useState<DialogPartner[]>([]);
  const [friends, setFriends] = useState<FriendInfoLite[]>([]);
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [messages, setMessages] = useState<DialogMessage[]>([]);
  const [text, setText] = useState('');
  const [loading, setLoading] = useState(true);
  const [messagesLoading, setMessagesLoading] = useState(false);
  const [sending, setSending] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const messagesEndRef = useRef<HTMLDivElement | null>(null);

  const loadPartners = useCallback(async () => {
    setPartners(await dialogApi.partners());
  }, []);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const [partnersData, friendsData] = await Promise.all([dialogApi.partners(), friendsApi.get()]);
        if (cancelled) return;
        setPartners(partnersData);
        setFriends(friendsData.friends);
      } catch (e) {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Не удалось загрузить диалоги');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const loadMessages = useCallback(async (partnerId: number) => {
    setMessagesLoading(true);
    try {
      setMessages(await dialogApi.list(partnerId));
    } catch (e) {
      setMessages([]);
      setError(e instanceof Error ? e.message : 'Не удалось загрузить сообщения');
    } finally {
      setMessagesLoading(false);
    }
  }, []);

  const selectPartner = useCallback(
    (partnerId: number) => {
      setError(null);
      setMenuOpen(false);
      setSelectedId(partnerId);
      void loadMessages(partnerId);
    },
    [loadMessages],
  );

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ block: 'end' });
  }, [messages]);

  const nameFor = (id: number): string => {
    const partner = partners.find((p) => p.id === id);
    if (partner) return partner.name;
    const friend = friends.find((f) => f.user.id === id);
    return friend ? friend.user.name : `Пользователь #${id}`;
  };

  const handleSend = async (e: FormEvent) => {
    e.preventDefault();
    const trimmed = text.trim();
    if (selectedId === null || !trimmed || sending) return;
    setSending(true);
    setError(null);
    try {
      await dialogApi.send(selectedId, trimmed);
      setText('');
      await loadMessages(selectedId);
      await loadPartners();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось отправить сообщение');
    } finally {
      setSending(false);
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

  return (
    <>
      {error && <div className="alert alert-danger">{error}</div>}
      <div className="row g-3">
        <div className="col-md-4">
          <div className="card">
            <div className="card-header d-flex justify-content-between align-items-center">
              <span>Диалоги</span>
              <div className="position-relative">
                <button
                  type="button"
                  className="btn btn-sm btn-outline-primary"
                  title="Написать другу"
                  onClick={() => setMenuOpen((open) => !open)}
                >
                  <i className="fa fa-pencil-alt"></i>
                </button>
                {menuOpen && (
                  <ul className="dropdown-menu show" style={{ right: 0, left: 'auto' }}>
                    {friends.length > 0 ? (
                      friends.map((friend) => (
                        <li key={friend.user.id}>
                          <button
                            type="button"
                            className="dropdown-item"
                            onClick={() => selectPartner(friend.user.id)}
                          >
                            {friend.user.name}
                          </button>
                        </li>
                      ))
                    ) : (
                      <li>
                        <span className="dropdown-item-text text-muted">Нет друзей</span>
                      </li>
                    )}
                  </ul>
                )}
              </div>
            </div>
            {partners.length > 0 ? (
              <ul className="list-group list-group-flush">
                {partners.map((partner) => (
                  <li key={partner.id}>
                    <button
                      type="button"
                      className={`list-group-item list-group-item-action w-100 text-start ${
                        selectedId === partner.id ? 'active' : ''
                      }`}
                      onClick={() => selectPartner(partner.id)}
                    >
                      <i className="fa fa-user me-2"></i>
                      {partner.name}
                    </button>
                  </li>
                ))}
              </ul>
            ) : (
              <div className="card-body text-muted">У вас пока нет диалогов</div>
            )}
          </div>
        </div>

        <div className="col-md-8">
          <div className="card">
            {selectedId === null ? (
              <div className="card-body text-muted">Выберите диалог или напишите другу</div>
            ) : (
              <>
                <div className="card-header">
                  <i className="fa fa-user me-2"></i>
                  {nameFor(selectedId)}
                </div>
                <div className="card-body dialog-messages">
                  {messagesLoading ? (
                    <div className="d-flex justify-content-center">
                      <div className="spinner-border spinner-border-sm" role="status">
                        <span className="visually-hidden">Загрузка...</span>
                      </div>
                    </div>
                  ) : messages.length > 0 ? (
                    <>
                      {messages.map((message, index) => {
                        const isMine = message.from !== String(selectedId);
                        return (
                          <div
                            key={index}
                            className={`dialog-bubble ${isMine ? 'dialog-bubble-mine' : 'dialog-bubble-theirs'}`}
                          >
                            {message.text}
                          </div>
                        );
                      })}
                      <div ref={messagesEndRef}></div>
                    </>
                  ) : (
                    <div className="text-muted">Пока нет сообщений</div>
                  )}
                </div>
                <div className="card-footer">
                  <form onSubmit={handleSend} className="d-flex gap-2">
                    <input
                      type="text"
                      className="form-control"
                      placeholder="Введите сообщение..."
                      value={text}
                      onChange={(e) => setText(e.target.value)}
                      disabled={sending}
                    />
                    <button type="submit" className="btn btn-primary" disabled={sending || !text.trim()}>
                      <i className="fa fa-paper-plane"></i>
                    </button>
                  </form>
                </div>
              </>
            )}
          </div>
        </div>
      </div>
    </>
  );
}
