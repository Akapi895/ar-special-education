import { lazy } from 'react';
import { Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../providers/AuthProvider';
import RouteSuspense from '../components/common/RouteSuspense';
import { ChildProvider } from '../providers/ChildProvider';
import Sidebar from '../components/layout/Sidebar';

// Lazy load pages for better performance
const LandingPage = lazy(() => import('../pages/public/LandingPage'));
const AuthPage = lazy(() => import('../pages/public/AuthPage'));
const OnboardingPage = lazy(() => import('../pages/public/OnboardingPage'));

// Parent pages - lazy loaded
const DashboardPage = lazy(() => import('../pages/parents/DashboardPage'));
const ExerciseLibraryPage = lazy(() => import('../pages/parents/ExerciseLibraryPage'));
const JournalPage = lazy(() => import('../pages/parents/JournalPage'));
const MethodsPage = lazy(() => import('../pages/parents/MethodsPage'));
const SettingsPage = lazy(() => import('../pages/parents/SettingsPage'));

// Parent Layout with ChildProvider and Sidebar
const ParentLayout = () => (
  <ChildProvider>
    <div className="flex min-h-screen bg-light-bg">
      <Sidebar />
      <main className="flex-1 ml-20 overflow-y-auto transition-all duration-300">
        <div className="animate-fade-in">
          <Outlet />
        </div>
      </main>
    </div>
  </ChildProvider>
);

export const AppRouter = () => {
  const { isAuthenticated, user } = useAuth();
  const isParentAuthenticated = isAuthenticated && user?.role === 'parent';

  return (
    <Routes>
      {/* Public Routes - Wrapped in Suspense */}
      <Route path="/" element={<RouteSuspense><LandingPage /></RouteSuspense>} />
      <Route path="/login" element={<RouteSuspense><AuthPage /></RouteSuspense>} />
      <Route path="/register" element={<RouteSuspense><AuthPage /></RouteSuspense>} />
      <Route path="/onboarding" element={<RouteSuspense><OnboardingPage /></RouteSuspense>} />

      {/* Parent Routes - Protected & Lazy Loaded */}
      {isParentAuthenticated && (
        <Route path="/parent" element={<ParentLayout />}>
          <Route path="dashboard" element={<RouteSuspense><DashboardPage /></RouteSuspense>} />
          <Route path="exercises" element={<RouteSuspense><ExerciseLibraryPage /></RouteSuspense>} />
          <Route path="journal" element={<RouteSuspense><JournalPage /></RouteSuspense>} />
          <Route path="methods" element={<RouteSuspense><MethodsPage /></RouteSuspense>} />
          <Route path="settings" element={<RouteSuspense><SettingsPage /></RouteSuspense>} />
          <Route index element={<Navigate to="/parent/dashboard" replace />} />
        </Route>
      )}

      {/* Redirect logic - only redirect authenticated users */}
      <Route
        path="*"
        element={
          isParentAuthenticated
            ? <Navigate to="/parent/dashboard" replace />
            : <Navigate to="/" replace />
        }
      />
    </Routes>
  );
};

export default AppRouter;