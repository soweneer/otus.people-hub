import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export function Layout() {
  const { authenticated, email, signOut } = useAuth();
  const navigate = useNavigate();

  const handleSignOut = async () => {
    await signOut();
    navigate('/signin');
  };

  return (
    <>
      <header>
        <nav className="navbar navbar-expand-sm navbar-light bg-white border-bottom box-shadow mb-3">
          <div className="container">
            <button
              className="navbar-toggler"
              type="button"
              data-bs-toggle="collapse"
              data-bs-target=".navbar-collapse"
              aria-controls="navbarSupportedContent"
              aria-expanded="false"
              aria-label="Toggle navigation"
            >
              <span className="navbar-toggler-icon"></span>
            </button>
            <div className="navbar-collapse collapse d-sm-inline-flex flex-sm-row-reverse">
              {!authenticated ? (
                <ul className="navbar-nav flex-grow-1">
                  <li className="nav-item">
                    <Link to="/signup" className="nav-link text-dark">
                      <i className="fa fa-user-plus"></i> Присоединиться
                    </Link>
                  </li>
                  <li className="nav-item">
                    <Link to="/signin" className="nav-link text-dark">
                      <i className="fa fa-sign-in-alt"></i> Войти
                    </Link>
                  </li>
                </ul>
              ) : (
                <ul className="navbar-nav flex-grow-1">
                  <li className="nav-item dropdown">
                    <a
                      tabIndex={-1}
                      href="#"
                      onClick={(e) => e.preventDefault()}
                      className="nav-link text-dark dropdown-toggle"
                      data-bs-toggle="dropdown"
                    >
                      <i className="fa fa-user"></i> {email}
                    </a>
                    <ul className="dropdown-menu">
                      <li className="dropdown-item">
                        <Link to="/profile" className="nav-link">
                          <i className="fa fa-user-edit"></i> Профиль
                        </Link>
                      </li>
                      <li className="dropdown-divider"></li>
                      <li className="dropdown-item">
                        <a
                          href="#"
                          className="nav-link"
                          onClick={(e) => {
                            e.preventDefault();
                            void handleSignOut();
                          }}
                        >
                          <i className="fa fa-sign-out-alt"></i> Выйти
                        </a>
                      </li>
                    </ul>
                  </li>
                </ul>
              )}
              <ul className="navbar-nav flex-grow-1">
                {authenticated && (
                  <>
                    <li className="nav-item">
                      <NavLink to="/people" className="nav-link text-dark">
                        <i className="fa fa-users"></i> Люди
                      </NavLink>
                    </li>
                    <li className="nav-item">
                      <NavLink to="/friends" className="nav-link text-dark">
                        <i className="fa fa-user-friends"></i> Мои друзья
                      </NavLink>
                    </li>
                  </>
                )}
              </ul>
            </div>
          </div>
        </nav>
      </header>
      <div className="container">
        <main role="main" className="pb-3">
          <Outlet />
        </main>
      </div>
      <footer className="border-top footer text-muted">
        <div className="container">&copy; 2026 - People Hub</div>
      </footer>
    </>
  );
}
