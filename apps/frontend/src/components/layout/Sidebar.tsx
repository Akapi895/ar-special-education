import { useEffect, useState } from 'react';
import { NavLink, useLocation, useNavigate } from 'react-router-dom';
import {
  Calculator,
  LayoutDashboard,
  BookOpen,
  Heart,
  BookMarked,
  Settings,
  LogOut,
  Menu,
  X,
  MoonStar,
  SunMedium,
} from 'lucide-react';
import { useAuth } from '../../providers/AuthProvider';
import { useTheme } from '../../providers/ThemeProvider';

const Sidebar = () => {
  const { logout } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const navigate = useNavigate();
  const location = useLocation();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  useEffect(() => {
    setIsMobileMenuOpen(false);
  }, [location.pathname]);

  const handleLogout = async () => {
    try {
      await logout();
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      setIsMobileMenuOpen(false);
      navigate('/', { replace: true });
    }
  };
  
  const navItems = [
    {
      to: '/parent/dashboard',
      icon: LayoutDashboard,
      label: 'Dashboard',
    },
    {
      to: '/parent/exercises',
      icon: BookOpen,
      label: 'Bài tập',
    },
    {
      to: '/parent/journal',
      icon: Heart,
      label: 'Nhật ký',
    },
    {
      to: '/parent/methods',
      icon: BookMarked,
      label: 'Phương pháp',
    },
    {
      to: '/parent/settings',
      icon: Settings,
      label: 'Cài đặt',
    },
  ];

  return (
    <>
      <header className="fixed inset-x-0 top-0 z-50 flex h-16 items-center justify-between border-b border-white/10 bg-gradient-primary px-4 backdrop-blur md:hidden">
        <button
          type="button"
          onClick={() => setIsMobileMenuOpen(true)}
          className="inline-flex h-10 w-10 items-center justify-center rounded-xl text-white transition-colors hover:bg-white/10"
          aria-label="Mở menu điều hướng"
          aria-expanded={isMobileMenuOpen}
          aria-controls="parent-sidebar-mobile"
        >
          <Menu className="h-5 w-5" />
        </button>

        <div className="flex items-center gap-2">
          <Calculator className="h-7 w-7 text-white" strokeWidth={2} />
          <span className="text-sm font-semibold text-white">MathMate</span>
        </div>

        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={toggleTheme}
            className="inline-flex h-10 w-10 items-center justify-center rounded-xl text-white transition-colors hover:bg-white/10"
            aria-label={theme === 'dark' ? 'Chuyển sang chế độ sáng' : 'Chuyển sang chế độ tối'}
          >
            {theme === 'dark' ? <SunMedium className="h-4 w-4" /> : <MoonStar className="h-4 w-4" />}
          </button>
          <button
            type="button"
            onClick={handleLogout}
            className="inline-flex h-10 items-center justify-center rounded-xl px-3 text-sm font-medium text-white transition-colors hover:bg-white/10"
          >
            Đăng xuất
          </button>
        </div>
      </header>

      <div
        className={`fixed inset-0 z-40 bg-black/40 transition-opacity md:hidden ${
          isMobileMenuOpen ? 'opacity-100' : 'pointer-events-none opacity-0'
        }`}
        onClick={() => setIsMobileMenuOpen(false)}
        aria-hidden="true"
      />

      <aside className="fixed left-0 top-0 z-50 hidden h-screen w-20 flex-col items-center bg-gradient-primary py-6 shadow-lg md:flex">
        <div className="mb-8">
          <div className="w-12 h-12 rounded-xl flex items-center justify-center bg-gradient-primary shadow-md">
            <Calculator className="w-6 h-6 text-white" strokeWidth={2} />
          </div>
        </div>

        <nav className="flex-1 flex flex-col gap-3 w-full px-3">
          {navItems.map((item) => {
            const Icon = item.icon;
            return (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  `group relative flex items-center justify-center w-full h-12 rounded-xl ${
                    isActive ? 'bg-[#e3f1ff]' : 'hover:bg-white/10'
                  }`
                }
              >
                {({ isActive }) => (
                  <>
                    <Icon
                      className={`w-5 h-5 ${
                        isActive ? 'text-[#1e3a8a]' : 'text-white'
                      }`}
                      strokeWidth={2}
                    />

                    <span className="absolute left-full ml-4 whitespace-nowrap rounded-lg bg-slate-800 px-3 py-2 text-sm text-white opacity-0 invisible shadow-lg pointer-events-none group-hover:opacity-100 group-hover:visible">
                      {item.label}
                      <span className="absolute right-full top-1/2 -translate-y-1/2 border-4 border-transparent border-r-slate-800" />
                    </span>
                  </>
                )}
              </NavLink>
            );
          })}
        </nav>

        <button
          type="button"
          className="group relative mt-2 flex h-12 w-full items-center justify-center rounded-xl px-3 hover:bg-white/10"
          onClick={handleLogout}
        >
          <LogOut
            className="w-5 h-5 text-white"
            strokeWidth={2}
          />

          <span className="absolute left-full ml-4 whitespace-nowrap rounded-lg bg-slate-800 px-3 py-2 text-sm text-white opacity-0 invisible shadow-lg pointer-events-none group-hover:opacity-100 group-hover:visible">
            Đăng xuất
            <span className="absolute right-full top-1/2 -translate-y-1/2 border-4 border-transparent border-r-slate-800" />
          </span>
        </button>

        <button
          type="button"
          className="group relative mt-2 flex h-12 w-full items-center justify-center rounded-xl px-3 hover:bg-white/10"
          onClick={toggleTheme}
          aria-label={theme === 'dark' ? 'Chuyển sang chế độ sáng' : 'Chuyển sang chế độ tối'}
        >
          {theme === 'dark' ? (
            <SunMedium className="h-5 w-5 text-white" strokeWidth={2} />
          ) : (
            <MoonStar className="h-5 w-5 text-white" strokeWidth={2} />
          )}

          <span className="absolute left-full ml-4 whitespace-nowrap rounded-lg bg-slate-800 px-3 py-2 text-sm text-white opacity-0 invisible shadow-lg pointer-events-none group-hover:opacity-100 group-hover:visible">
            {theme === 'dark' ? 'Chế độ sáng' : 'Chế độ tối'}
            <span className="absolute right-full top-1/2 -translate-y-1/2 border-4 border-transparent border-r-slate-800" />
          </span>
        </button>
      </aside>

      <aside
        id="parent-sidebar-mobile"
        className={`fixed inset-y-0 left-0 z-50 flex w-72 flex-col bg-gradient-primary px-4 py-5 shadow-2xl transition-transform duration-300 md:hidden ${
          isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
        aria-hidden={!isMobileMenuOpen}
      >
          <div className="mb-6 flex items-center justify-between">
          <div className="flex items-center gap-2 text-white">
            <Calculator className="h-8 w-8" strokeWidth={2} />
            <span className="text-lg font-semibold">MathMate Support</span>
          </div>
          <button
            type="button"
            onClick={() => setIsMobileMenuOpen(false)}
            className="inline-flex h-10 w-10 items-center justify-center rounded-xl text-white transition-colors hover:bg-white/10"
            aria-label="Đóng menu"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <nav className="flex flex-1 flex-col gap-2">
          {navItems.map((item) => {
            const Icon = item.icon;
            return (
              <NavLink
                key={item.to}
                to={item.to}
                onClick={() => setIsMobileMenuOpen(false)}
                className={({ isActive }) =>
                  `flex items-center gap-3 rounded-2xl px-4 py-3 text-sm font-medium transition-colors ${
                    isActive ? 'bg-[#e3f1ff] text-[#1e3a8a]' : 'text-white hover:bg-white/10'
                  }`
                }
              >
                {({ isActive }) => (
                  <>
                    <Icon className={`h-5 w-5 ${isActive ? 'text-[#1e3a8a]' : 'text-white'}`} strokeWidth={2} />
                    <span>{item.label}</span>
                  </>
                )}
              </NavLink>
            );
          })}
        </nav>

        <button
          type="button"
          onClick={handleLogout}
          className="mt-4 inline-flex items-center justify-center gap-2 rounded-2xl border border-white/10 px-4 py-3 text-sm font-semibold text-white transition-colors hover:bg-white/10"
        >
          <LogOut className="h-4 w-4" />
          Đăng xuất
        </button>

        <button
          type="button"
          onClick={toggleTheme}
          className="mt-3 inline-flex items-center justify-center gap-2 rounded-2xl border border-white/10 px-4 py-3 text-sm font-semibold text-white transition-colors hover:bg-white/10"
          aria-label={theme === 'dark' ? 'Chuyển sang chế độ sáng' : 'Chuyển sang chế độ tối'}
        >
          {theme === 'dark' ? <SunMedium className="h-4 w-4" /> : <MoonStar className="h-4 w-4" />}
          {theme === 'dark' ? 'Chế độ sáng' : 'Chế độ tối'}
        </button>
      </aside>
    </>
  );
};

export default Sidebar;
