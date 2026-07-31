import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';
import '@fortawesome/fontawesome-free/css/all.min.css';
import './site.css';
import { App } from './App';
import { AuthProvider } from './auth/AuthContext';
import { UnreadProvider } from './counters/UnreadContext';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <UnreadProvider>
          <App />
        </UnreadProvider>
      </AuthProvider>
    </BrowserRouter>
  </StrictMode>,
);
