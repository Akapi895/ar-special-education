import { lazy } from 'react';
import { Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { AnimatePresence, motion } from 'framer-motion';
import { useLocation } from 'react-router-dom';
import { useAuth } from '../providers/AuthProvider';
import RouteSuspense from '../components/common/RouteSuspense';
import { ChildProvider } from '../providers/ChildProvider';
import Sidebar from '../components/layout/Sidebar';
import Breadcrumbs from '../components/common/Breadcrumbs';

// Lazy load pages for better performance
const LandingPage = lazy(() => import('../pages/public/LandingPage'));
const AuthPage = lazy(() => import('../pages/public/AuthPage'));
const OnboardingPage = lazy(() => import('../pages/public/OnboardingPage'));
const ChildLoginPage = lazy(() => import('../pages/public/ChildLoginPage'));

// Parent pages - lazy loaded
const DashboardPage = lazy(() => import('../pages/parents/DashboardPage'));
const ExerciseLibraryPage = lazy(() => import('../pages/parents/ExerciseLibraryPage'));
const JournalPage = lazy(() => import('../pages/parents/JournalPage'));
const MethodsPage = lazy(() => import('../pages/parents/MethodsPage'));
const SettingsPage = lazy(() => import('../pages/parents/SettingsPage'));

const ParentLayout = () => (
  <ChildProvider>
    <div className="flex min-h-screen bg-(--app-bg) transition-colors duration-300">
      <Sidebar />
      <main className="flex-1 min-h-screen overflow-y-auto pt-16 transition-all duration-300 md:ml-20 md:pt-0">
        <div className="animate-fade-in px-4 py-5 sm:px-6 lg:px-8">
          <Breadcrumbs />
          <Outlet />
        </div>
      </main>
    </div>
  </ChildProvider>
);

const ProtectedParentLayout = () => {
  const { isAuthenticated, user } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (user?.role === 'child') {
    return <Navigate to="/child-login" replace />;
  }

  if (user?.role !== 'parent') {
    return <Navigate to="/" replace />;
  }

  return <ParentLayout />;
};

export const AppRouter = () => {
  const { isAuthenticated, user } = useAuth();
  const isParentAuthenticated = isAuthenticated && user?.role === 'parent';
  const location = useLocation();

  return (
    <AnimatePresence mode="wait" initial={false}>
      <motion.div
        key={location.pathname}
        initial={{ opacity: 0, y: 8 }}
        animate={{ opacity: 1, y: 0 }}
        exit={{ opacity: 0, y: -8 }}
        transition={{ duration: 0.18, ease: 'easeOut' }}
        className="min-h-screen"
      >
        <Routes location={location}>
          {/* Public Routes - Wrapped in Suspense */}
          <Route path="/" element={<RouteSuspense><LandingPage /></RouteSuspense>} />
          <Route path="/login" element={<RouteSuspense><AuthPage /></RouteSuspense>} />
          <Route path="/register" element={<RouteSuspense><AuthPage /></RouteSuspense>} />
          <Route path="/onboarding" element={<RouteSuspense><OnboardingPage /></RouteSuspense>} />
          <Route path="/child-login" element={<RouteSuspense><ChildLoginPage /></RouteSuspense>} />

          {/* Parent Routes - Protected & Lazy Loaded */}
          <Route path="/parent" element={<ProtectedParentLayout />}>
            <Route index element={<Navigate to="/parent/dashboard" replace />} />
            <Route path="dashboard" element={<RouteSuspense><DashboardPage /></RouteSuspense>} />
            <Route path="exercises" element={<RouteSuspense><ExerciseLibraryPage /></RouteSuspense>} />
            <Route path="journal" element={<RouteSuspense><JournalPage /></RouteSuspense>} />
            <Route path="methods" element={<RouteSuspense><MethodsPage /></RouteSuspense>} />
            <Route path="settings" element={<RouteSuspense><SettingsPage /></RouteSuspense>} />
          </Route>

          {/* Redirect logic - only redirect authenticated users */}
          <Route
            path="*"
            element={
              isParentAuthenticated
                ? <Navigate to="/parent/dashboard" replace />
                : <Navigate to="/login" replace />
            }
          />
        </Routes>
      </motion.div>
    </AnimatePresence>
  );
};

export default AppRouter;