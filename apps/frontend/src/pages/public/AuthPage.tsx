import { useState, useEffect } from 'react';
import { useNavigate, Link, useLocation } from 'react-router-dom';
import { Sparkles, LogIn, UserPlus, Lock } from 'lucide-react';
import { z } from 'zod';
import Button from '../../components/ui/Button';
import Input from '../../components/ui/Input';
import PasswordInput from '../../components/ui/PasswordInput';
import { useAuth } from '../../providers/AuthProvider';
import type { LoginCredentials, RegisterCredentials } from '../../types/auth.types';

type AuthMode = 'login' | 'register';

const emailSchema = z.string().trim().min(1, 'Vui lòng nhập email').email('Email không hợp lệ');
const passwordSchema = z.string().min(1, 'Vui lòng nhập mật khẩu').min(6, 'Mật khẩu phải có ít nhất 6 ký tự');

const loginSchema = z.object({
  email: emailSchema,
  password: passwordSchema,
});

const registerSchema = z.object({
  displayName: z.string().trim().min(1, 'Vui lòng nhập tên'),
  email: emailSchema,
  password: passwordSchema,
  confirmPassword: z.string().min(1, 'Vui lòng xác nhận mật khẩu'),
}).refine((data) => data.password === data.confirmPassword, {
  path: ['confirmPassword'],
  message: 'Mật khẩu không khớp',
});

const AuthPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { login, register } = useAuth();
  
  const initialMode: AuthMode = location.pathname === '/register' ? 'register' : 'login';
  const [mode, setMode] = useState<AuthMode>(initialMode);
  const [isLoading, setIsLoading] = useState(false);
  const [isAnimating, setIsAnimating] = useState(false);

  useEffect(() => {
    const newMode: AuthMode = location.pathname === '/register' ? 'register' : 'login';
    setMode(newMode);
  }, [location.pathname]);

  const [loginCredentials, setLoginCredentials] = useState<LoginCredentials>({ email: '', password: '' });
  const [loginErrors, setLoginErrors] = useState<Partial<Record<keyof LoginCredentials, string | undefined>>>({});
  const [registerCredentials, setRegisterCredentials] = useState<RegisterCredentials>({ email: '', password: '', confirmPassword: '', displayName: '' });
  const [registerErrors, setRegisterErrors] = useState<Partial<Record<keyof RegisterCredentials, string | undefined>>>({});

  const mapZodErrors = <T extends string>(issues: z.ZodIssue[]) => issues.reduce((acc, issue) => {
    const key = issue.path[0] as T | undefined;
    if (key && !acc[key]) {
      acc[key] = issue.message;
    }
    return acc;
  }, {} as Partial<Record<T, string | undefined>>);

  const handleLoginSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const parsed = loginSchema.safeParse(loginCredentials);
    if (!parsed.success) {
      setLoginErrors(mapZodErrors<keyof LoginCredentials>(parsed.error.issues));
      return;
    }
    
    try {
      setIsLoading(true);
      await login(loginCredentials);
      navigate('/parent/dashboard', { replace: true });
    } catch {
      setLoginErrors({ password: 'Đăng nhập thất bại. Vui lòng thử lại.' });
    } finally {
      setIsLoading(false);
    }
  };

  const handleRegisterSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const parsed = registerSchema.safeParse(registerCredentials);
    if (!parsed.success) {
      setRegisterErrors(mapZodErrors<keyof RegisterCredentials>(parsed.error.issues));
      return;
    }
    
    try {
      setIsLoading(true);
      await register({
        email: registerCredentials.email,
        password: registerCredentials.password,
        displayName: registerCredentials.displayName,
      });
      navigate('/onboarding', { replace: true });
    } catch {
      setRegisterErrors({ email: 'Đăng ký thất bại. Vui lòng thử lại.' });
    } finally {
      setIsLoading(false);
    }
  };

  const switchMode = (newMode: AuthMode) => {
    if (isAnimating) return;
    setIsAnimating(true);
    setLoginErrors({});
    setRegisterErrors({});
    setMode(newMode);
    navigate(newMode === 'login' ? '/login' : '/register', { replace: true });
    setTimeout(() => setIsAnimating(false), 700);
  };

  return (
    <div className="min-h-screen bg-slate-50 flex items-center justify-center p-4 py-10 relative overflow-hidden">
      {/* Background Decoration */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute -top-1/4 -left-1/4 w-[50%] h-[50%] bg-blue-500 rounded-full blur-[100px] opacity-30" />
        <div className="absolute bottom-0 right-0 w-[40%] h-[40%] bg-purple-500 rounded-full blur-[100px] opacity-20" />
      </div>

      {/* Main Card */}
      <div className="relative bg-white rounded-4xl shadow-2xl w-full max-w-[1000px] min-h-[650px] overflow-hidden flex flex-col md:flex-row z-10">
        
        {/* HEADER MOBILE */}
        <div className="md:hidden p-6 pb-0 text-center">
          <div className="inline-flex items-center justify-center w-12 h-12 rounded-xl bg-[#06325a] mb-2">
            <Sparkles className="w-6 h-6 text-white" />
          </div>
          <h1 className="text-2xl font-bold text-[#06325a]">MathMate Support</h1>
        </div>

        {/* =========================================
            LEFT SIDE: LOGIN FORM
           ========================================= */}
        <div className={`w-full md:w-1/2 p-8 md:p-12 flex flex-col justify-center transition-all duration-700 absolute md:relative top-0 left-0 h-full bg-white
            ${mode === 'login' 
                ? 'z-10 opacity-100 translate-x-0 pointer-events-auto' 
                : 'z-0 opacity-0 pointer-events-none'
            }
            ${mode === 'register' && 'hidden md:flex'} 
        `}>
            <div className="mb-8">
                <h2 className="text-3xl font-bold text-[#06325a] mb-2">Đăng nhập</h2>
                <p className="text-gray-500">Chào mừng bạn quay lại!</p>
            </div>

            <form onSubmit={handleLoginSubmit} className="space-y-5 relative z-10">
                <div>
                  <Input
                      label="Email"
                      type="email"
                      value={loginCredentials.email}
                      onChange={(e) => {
                        setLoginCredentials({ ...loginCredentials, email: e.target.value });
                        if (loginErrors.email) setLoginErrors({ ...loginErrors, email: undefined });
                      }}
                      placeholder="Nhập email của bạn"
                      fullWidth
                      className="bg-gray-50"
                  />
                  {loginErrors.email && (
                    <p className="text-red-500 text-sm mt-1">{loginErrors.email}</p>
                  )}
                </div>
                <div>
                  <PasswordInput
                      label="Mật khẩu"
                      value={loginCredentials.password}
                      onChange={(e) => {
                        setLoginCredentials({ ...loginCredentials, password: e.target.value });
                        if (loginErrors.password) setLoginErrors({ ...loginErrors, password: undefined });
                      }}
                      placeholder="Nhập mật khẩu"
                      fullWidth
                      className="bg-gray-50"
                  />
                  {loginErrors.password && (
                    <p className="text-red-500 text-sm mt-1">{loginErrors.password}</p>
                  )}
                </div>
                
                <div className="flex justify-between items-center text-sm">
                    <label className="flex items-center gap-2 text-gray-600 cursor-pointer">
                        <input type="checkbox" className="rounded border-gray-300 text-[#06325a]" />
                        Ghi nhớ đăng nhập
                    </label>
                    <Link to="/forgot-password" className="text-[#3498db] font-semibold">
                        Quên mật khẩu?
                    </Link>
                </div>

                <Button
                    type="submit"
                    fullWidth
                    size="lg"
                    disabled={isLoading}
                    className="bg-[#06325a] hover:bg-[#052848] text-white transition-all rounded-xl py-3 shadow-lg hover:shadow-xl"
                    icon={<LogIn className="w-5 h-5" />}
                >
                    {isLoading ? 'Đang đăng nhập...' : 'Đăng nhập'}
                </Button>
            </form>
        </div>

        {/* =========================================
            RIGHT SIDE: REGISTER FORM
           ========================================= */}
        <div className={`w-full md:w-1/2 p-8 md:p-12 flex flex-col justify-center transition-all duration-700 absolute md:relative top-0 right-0 h-full bg-white
            ${mode === 'register'
                ? 'z-10 opacity-100 translate-x-0 pointer-events-auto' 
                : 'z-0 opacity-0 pointer-events-none'
            }
            ${mode === 'login' && 'hidden md:flex'}
        `}>
            <div className="mb-6">
                <h2 className="text-3xl font-bold text-[#06325a] mb-2">Đăng ký</h2>
                <p className="text-gray-500">
                  Tham gia MathMate Support để hỗ trợ con học toán hiệu quả
                </p>
            </div>

            <form onSubmit={handleRegisterSubmit} className="space-y-4 relative z-10">
                <div>
                  <Input
                      label="Tên"
                      value={registerCredentials.displayName}
                      onChange={(e) => {
                        setRegisterCredentials({ ...registerCredentials, displayName: e.target.value });
                        if (registerErrors.displayName) setRegisterErrors({ ...registerErrors, displayName: undefined });
                      }}
                      placeholder="Nhập tên của bạn"
                      fullWidth
                      className="bg-gray-50"
                  />
                  {registerErrors.displayName && (
                    <p className="text-red-500 text-sm mt-1">{registerErrors.displayName}</p>
                  )}
                </div>
                <div>
                  <Input
                      label="Email"
                      type="email"
                      value={registerCredentials.email}
                      onChange={(e) => {
                        setRegisterCredentials({ ...registerCredentials, email: e.target.value });
                        if (registerErrors.email) setRegisterErrors({ ...registerErrors, email: undefined });
                      }}
                      placeholder="Nhập email của bạn"
                      fullWidth
                      className="bg-gray-50"
                  />
                  {registerErrors.email && (
                    <p className="text-red-500 text-sm mt-1">{registerErrors.email}</p>
                  )}
                </div>
                <div>
                  <PasswordInput
                      label="Mật khẩu"
                      value={registerCredentials.password}
                      onChange={(e) => {
                        setRegisterCredentials({ ...registerCredentials, password: e.target.value });
                        if (registerErrors.password) setRegisterErrors({ ...registerErrors, password: undefined });
                      }}
                      placeholder="Ít nhất 6 ký tự"
                      fullWidth
                      className="bg-gray-50"
                  />
                  {registerErrors.password && (
                    <p className="text-red-500 text-sm mt-1">{registerErrors.password}</p>
                  )}
                </div>
                <div>
                  <PasswordInput
                      label="Xác nhận mật khẩu"
                      value={registerCredentials.confirmPassword || ''}
                      onChange={(e) => {
                        setRegisterCredentials({ ...registerCredentials, confirmPassword: e.target.value });
                        if (registerErrors.confirmPassword) setRegisterErrors({ ...registerErrors, confirmPassword: undefined });
                      }}
                      placeholder="Nhập lại mật khẩu"
                      fullWidth
                      className="bg-gray-50"
                  />
                  {registerErrors.confirmPassword && (
                    <p className="text-red-500 text-sm mt-1">{registerErrors.confirmPassword}</p>
                  )}
                </div>
                
                <Button
                    type="submit"
                    fullWidth
                    size="lg"
                    disabled={isLoading}
                    className="bg-[#10b981] hover:bg-[#059669] text-white transition-all rounded-xl py-3 shadow-lg hover:shadow-xl"
                    icon={<UserPlus className="w-5 h-5" />}
                >
                    {isLoading ? 'Đang tạo tài khoản...' : 'Đăng ký'}
                </Button>
            </form>
        </div>

        {/* =========================================
            THE SLIDING OVERLAY
           ========================================= */}
        <div 
            className={`hidden md:block absolute top-0 h-full w-[55%] transition-all duration-700 ease-in-out z-20 shadow-2xl text-white overflow-hidden
            ${mode === 'login' ? 'left-1/2' : '-left-[5%]'}
            `}
            style={{
                background: 'linear-gradient(135deg, #06325a 0%, #3498db 100%)',
                clipPath: mode === 'login' 
                    ? 'polygon(15% 0, 100% 0, 100% 100%, 0% 100%)'
                    : 'polygon(0 0, 100% 0, 85% 100%, 0% 100%)'
            }}
        >
            {/* Overlay Background Decor */}
            <div className="absolute top-0 right-0 w-64 h-64 bg-white/10 rounded-full -translate-y-1/2 translate-x-1/2 blur-2xl" />
            <div className="absolute bottom-0 left-0 w-64 h-64 bg-white/10 rounded-full translate-y-1/2 -translate-x-1/2 blur-2xl" />

            {/* --- CONTENT: REGISTER PROMPT (Shown when Login is active) --- */}
            <div className={`absolute inset-0 flex flex-col items-center justify-center p-12 text-center transition-all duration-700 px-20
                ${mode === 'login' ? 'opacity-100 translate-x-0 delay-100 pointer-events-auto' : 'opacity-0 translate-x-[20%] pointer-events-none'}
            `}>
                <div className="inline-flex items-center justify-center w-20 h-20 rounded-3xl bg-white/10 backdrop-blur-md mb-6 border border-white/20 shadow-lg">
                    <UserPlus className="w-10 h-10 text-white" />
                </div>
                <h2 className="text-4xl font-bold mb-4 text-white">
                  Mới sử dụng?
                </h2>
                <p className="text-blue-100 mb-8 text-lg">
                  Tham gia MathMate Support để hỗ trợ con học toán hiệu quả
                </p>
                
                <button 
                    onClick={() => switchMode('register')}
                    className="relative z-50 bg-white text-[#06325a] hover:bg-blue-50 rounded-full px-10 py-3 font-bold text-lg shadow-lg transform transition hover:scale-105 cursor-pointer"
                >
                    Đăng ký ngay
                </button>
            </div>

            {/* --- CONTENT: LOGIN PROMPT (Shown when Register is active) --- */}
            <div className={`absolute inset-0 flex flex-col items-center justify-center p-12 text-center transition-all duration-700 px-20
                ${mode === 'register' ? 'opacity-100 translate-x-0 delay-100 pointer-events-auto' : 'opacity-0 -translate-x-[20%] pointer-events-none'}
            `}>
                <div className="inline-flex items-center justify-center w-20 h-20 rounded-3xl bg-white/10 backdrop-blur-md mb-6 border border-white/20 shadow-lg">
                    <Lock className="w-10 h-10 text-white" />
                </div>
                <h2 className="text-4xl font-bold mb-4 text-white">
                  Chào mừng trở lại!
                </h2>
                <p className="text-blue-100 mb-8 text-lg">
                  Đăng nhập để tiếp tục hành trình cùng con
                </p>
                
                <button 
                    onClick={() => switchMode('login')}
                    className="relative z-50 bg-white text-[#06325a] hover:bg-blue-50 rounded-full px-10 py-3 font-bold text-lg shadow-lg transform transition hover:scale-105 cursor-pointer"
                >
                    Đăng nhập
                </button>
            </div>
        </div>

      </div>
    </div>
  );
};

export default AuthPage;
