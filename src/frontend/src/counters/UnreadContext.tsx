import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { countersApi } from '../api/client';
import { useFeedSocket } from '../api/feedSocket';
import { useAuth } from '../auth/AuthContext';

interface UnreadContextValue {
  total: number;
  countFor: (partnerId: number) => number;
  clearFor: (partnerId: number) => void;
}

const UnreadContext = createContext<UnreadContextValue | null>(null);

const noop = () => {};

function UnreadSocket({ onUnread }: { onUnread: (partnerId: number, count: number) => void }) {
  useFeedSocket(noop, onUnread);
  return null;
}

export function UnreadProvider({ children }: { children: ReactNode }) {
  const { authenticated } = useAuth();
  const [counters, setCounters] = useState<Record<number, number>>({});

  useEffect(() => {
    if (!authenticated) {
      setCounters({});
      return;
    }

    let cancelled = false;
    countersApi
      .unread()
      .then((data) => {
        if (!cancelled) {
          setCounters(Object.fromEntries(data.counters.map((counter) => [counter.partnerId, counter.count])));
        }
      })
      .catch(() => {
        if (!cancelled) setCounters({});
      });

    return () => {
      cancelled = true;
    };
  }, [authenticated]);

  const onUnread = useCallback((partnerId: number, count: number) => {
    setCounters((prev) => ({ ...prev, [partnerId]: count }));
  }, []);

  const clearFor = useCallback((partnerId: number) => {
    setCounters((prev) => (prev[partnerId] ? { ...prev, [partnerId]: 0 } : prev));
  }, []);

  const value = useMemo<UnreadContextValue>(
    () => ({
      total: Object.values(counters).reduce((sum, count) => sum + count, 0),
      countFor: (partnerId: number) => counters[partnerId] ?? 0,
      clearFor,
    }),
    [counters, clearFor],
  );

  return (
    <UnreadContext.Provider value={value}>
      {authenticated && <UnreadSocket onUnread={onUnread} />}
      {children}
    </UnreadContext.Provider>
  );
}

export function useUnread(): UnreadContextValue {
  const context = useContext(UnreadContext);
  if (!context) {
    throw new Error('useUnread должен вызываться внутри UnreadProvider');
  }
  return context;
}
