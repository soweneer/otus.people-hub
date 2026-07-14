import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react';
import { authApi } from '../api/client';
import type { SignUpData } from '../api/types';

interface AuthState {
  loading: boolean;
  authenticated: boolean;
  email: string | null;
}

interface AuthContextValue extends AuthState {
  signIn: (email: string, password: string) => Promise<void>;
  signUp: (data: SignUpData) => Promise<void>;
  signOut: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({ loading: true, authenticated: false, email: null });

  useEffect(() => {
    let cancelled = false;
    authApi
      .me()
      .then((me) => {
        if (!cancelled) setState({ loading: false, authenticated: me.authenticated, email: me.email });
      })
      .catch(() => {
        if (!cancelled) setState({ loading: false, authenticated: false, email: null });
      });
    return () => {
      cancelled = true;
    };
  }, []);

  // Любой запрос, получивший 401, сбрасывает сессию на клиенте
  useEffect(() => {
    const onUnauthorized = () =>
      setState((prev) =>
        prev.authenticated ? { loading: false, authenticated: false, email: null } : prev,
      );
    window.addEventListener('auth:unauthorized', onUnauthorized);
    return () => window.removeEventListener('auth:unauthorized', onUnauthorized);
  }, []);

  const signIn = useCallback(async (email: string, password: string) => {
    const me = await authApi.signIn(email, password);
    setState({ loading: false, authenticated: me.authenticated, email: me.email });
  }, []);

  const signUp = useCallback(async (data: SignUpData) => {
    const me = await authApi.signUp(data);
    setState({ loading: false, authenticated: me.authenticated, email: me.email });
  }, []);

  const signOut = useCallback(async () => {
    await authApi.signOut();
    setState({ loading: false, authenticated: false, email: null });
  }, []);

  return (
    <AuthContext.Provider value={{ ...state, signIn, signUp, signOut }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
