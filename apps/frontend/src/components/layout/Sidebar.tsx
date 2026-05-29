import { NavLink } from 'react-router-dom';
import {
  Calculator,
  LayoutDashboard,
  BookOpen,
  Heart,
  BookMarked,
  Settings,
  LogOut,
} from 'lucide-react';
import { useAuth } from '../../providers/AuthProvider';

const Sidebar = () => {
  const { logout } = useAuth();
  
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
    <aside className="fixed left-0 top-0 h-screen w-20 bg-slate-900 flex flex-col items-center py-6 shadow-lg z-50">
      {/* Logo */}
      <div className="mb-8">
        <Calculator className="w-10 h-10 text-white" strokeWidth={2} />
      </div>

      {/* Navigation Items */}
      <nav className="flex-1 flex flex-col gap-3 w-full px-3">
        {navItems.map((item) => {
          const Icon = item.icon;
          return (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `group relative flex items-center justify-center w-full h-12 rounded-xl transition-all duration-200 ${
                  isActive 
                    ? 'bg-indigo-600' 
                    : 'hover:bg-white/10'
                }`
              }
            >
              <Icon
                className="w-5 h-5 text-white group-hover:scale-110 transition-transform"
                strokeWidth={2}
              />
              
              {/* Tooltip */}
              <span className="absolute left-full ml-4 px-3 py-2 bg-slate-800 text-white text-sm rounded-lg whitespace-nowrap opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 pointer-events-none shadow-lg">
                {item.label}
                <span className="absolute right-full top-1/2 -translate-y-1/2 border-4 border-transparent border-r-slate-800"></span>
              </span>
            </NavLink>
          );
        })}
      </nav>

      {/* Logout Button */}
      <button
        className="group relative flex items-center justify-center w-full h-12 rounded-xl transition-all duration-200 hover:bg-white/10 px-3 mt-2"
        onClick={async () => {
          try {
            await logout();
            window.location.href = '/';
          } catch (error) {
            console.error('Logout error:', error);
            window.location.href = '/';
          }
        }}
      >
        <LogOut
          className="w-5 h-5 text-white group-hover:scale-110 transition-transform"
          strokeWidth={2}
        />
        
        {/* Tooltip */}
        <span className="absolute left-full ml-4 px-3 py-2 bg-slate-800 text-white text-sm rounded-lg whitespace-nowrap opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 pointer-events-none shadow-lg">
          Đăng xuất
          <span className="absolute right-full top-1/2 -translate-y-1/2 border-4 border-transparent border-r-slate-800"></span>
        </span>
      </button>
    </aside>
  );
};

export default Sidebar;
