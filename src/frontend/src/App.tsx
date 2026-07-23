import { Navigate, Route, Routes } from 'react-router-dom';
import { useAuth } from './auth/AuthContext';
import { Layout } from './components/Layout';
import { DialogsPage } from './pages/DialogsPage';
import { FriendsPage } from './pages/FriendsPage';
import { PeoplePage } from './pages/PeoplePage';
import { ProfilePage } from './pages/ProfilePage';
import { SignInPage } from './pages/SignInPage';
import { SignUpPage } from './pages/SignUpPage';
import { UserPage } from './pages/UserPage';
import type { ReactNode } from 'react';

function RequireAuth({ children }: { children: ReactNode }) {
  const { loading, authenticated } = useAuth();
  if (loading) {
    return (
      <div className="d-flex justify-content-center mt-5">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Загрузка...</span>
        </div>
      </div>
    );
  }
  return authenticated ? children : <Navigate to="/signin" replace />;
}

function AnonymousOnly({ children }: { children: ReactNode }) {
  const { loading, authenticated } = useAuth();
  if (loading) return null;
  return authenticated ? <Navigate to="/people" replace /> : children;
}

export function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Navigate to="/people" replace />} />
        <Route
          path="/signin"
          element={
            <AnonymousOnly>
              <SignInPage />
            </AnonymousOnly>
          }
        />
        <Route
          path="/signup"
          element={
            <AnonymousOnly>
              <SignUpPage />
            </AnonymousOnly>
          }
        />
        <Route
          path="/people"
          element={
            <RequireAuth>
              <PeoplePage />
            </RequireAuth>
          }
        />
        <Route
          path="/users/:id"
          element={
            <RequireAuth>
              <UserPage />
            </RequireAuth>
          }
        />
        <Route
          path="/profile"
          element={
            <RequireAuth>
              <ProfilePage />
            </RequireAuth>
          }
        />
        <Route
          path="/friends"
          element={
            <RequireAuth>
              <FriendsPage />
            </RequireAuth>
          }
        />
        <Route
          path="/dialogs"
          element={
            <RequireAuth>
              <DialogsPage />
            </RequireAuth>
          }
        />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
