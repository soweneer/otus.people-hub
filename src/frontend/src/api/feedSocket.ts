import { useEffect, useRef, useState } from 'react';
import type { FeedPost, FeedPostedEvent, UnreadChangedEvent } from './types';

const FEED_SOCKET_PATH = '/post/feed/posted';
const MAX_RECONNECT_DELAY_MS = 30_000;

type SocketEvent = FeedPostedEvent | UnreadChangedEvent;

function parseEvent(data: unknown): SocketEvent | null {
  if (typeof data !== 'string') return null;

  try {
    return JSON.parse(data) as SocketEvent;
  } catch {
    return null;
  }
}

function isUnread(payload: SocketEvent): payload is UnreadChangedEvent {
  return (payload as UnreadChangedEvent).kind === 'unread';
}

export function useFeedSocket(
  onPosted: (post: FeedPost) => void,
  onUnread?: (partnerId: number, count: number, total: number) => void,
): boolean {
  const [connected, setConnected] = useState(false);
  const postedRef = useRef(onPosted);
  const unreadRef = useRef(onUnread);
  postedRef.current = onPosted;
  unreadRef.current = onUnread;

  useEffect(() => {
    let socket: WebSocket | null = null;
    let reconnectTimer: number | undefined;
    let attempt = 0;
    let disposed = false;

    const connect = () => {
      const scheme = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
      socket = new WebSocket(`${scheme}//${window.location.host}${FEED_SOCKET_PATH}`);

      socket.onopen = () => {
        attempt = 0;
        setConnected(true);
      };

      socket.onmessage = (event) => {
        const payload = parseEvent(event.data);
        if (!payload) return;

        if (isUnread(payload)) {
          unreadRef.current?.(Number(payload.partnerId), payload.count, payload.total);
          return;
        }

        postedRef.current({
          id: Number(payload.postId),
          text: payload.postText,
          authorUserId: Number(payload.author_user_id),
        });
      };

      socket.onerror = () => socket?.close();

      socket.onclose = () => {
        setConnected(false);
        if (disposed) return;
        attempt += 1;
        reconnectTimer = window.setTimeout(connect, Math.min(1000 * 2 ** attempt, MAX_RECONNECT_DELAY_MS));
      };
    };

    connect();

    return () => {
      disposed = true;
      window.clearTimeout(reconnectTimer);
      socket?.close();
    };
  }, []);

  return connected;
}
