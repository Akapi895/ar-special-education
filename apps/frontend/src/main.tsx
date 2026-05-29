import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App'
import { BrowserRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { AuthProvider } from './providers/AuthProvider'
import { ThemeProvider, initializeTheme } from './providers/ThemeProvider'
import ErrorBoundary from './components/common/ErrorBoundary'
import { Toaster } from 'react-hot-toast'
import './i18n'
import { installLocalizationRuntimePatches } from './i18n'
import './styles/index.css'
import { toastTheme } from './utils/toast'

// Create QueryClient instance with default options
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1, // Retry failed queries once
      refetchOnWindowFocus: false, // Don't refetch on window focus
      staleTime: 30000, // Consider data fresh for 30 seconds
    },
  },
});

installLocalizationRuntimePatches();
initializeTheme();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    {/* Error Boundary - Catch and handle React errors */}
    <ErrorBoundary>
      {/* React Router - Handle routing */}
      <BrowserRouter>
        {/* React Query - Handle server state */}
        <QueryClientProvider client={queryClient}>
          <ThemeProvider>
            {/* Auth Provider - Handle authentication */}
            <AuthProvider>
              <App />
              {/* Toast Notifications */}
              <Toaster {...toastTheme} />
            </AuthProvider>
          </ThemeProvider>
        </QueryClientProvider>
      </BrowserRouter>
    </ErrorBoundary>
  </React.StrictMode>,
)
