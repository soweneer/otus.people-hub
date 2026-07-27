import { useEffect, useRef, useState } from 'react';
import type { FeedPost, FeedPostedEvent } from './types';

const FEED_SOCKET_PATH = '/post/feed/posted';
const MAX_RECONNECT_DELAY_MS = 30_000;

function parsePosted(data: unknown): FeedPostedEvent | null {
  if (typeof data !== 'string') return null;

  try {
    return JSON.parse(data) as FeedPostedEvent;
  } catch {
    return null;
  }
}

export function useFeedSocket(onPosted: (post: FeedPost) => void): boolean {
  const [connected, setConnected] = useState(false);
  const handlerRef = useRef(onPosted);
  handlerRef.current = onPosted;

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
        const payload = parsePosted(event.data);
        if (!payload) return;

        handlerRef.current({
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
