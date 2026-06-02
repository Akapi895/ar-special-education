/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        // Primary Colors
        'primary': {
          DEFAULT: '#70c69d',
          50: '#eefaf3',
          100: '#dff6ea',
          200: '#bff0d5',
          300: '#9fe9c0',
          400: '#7fe2ab',
          500: '#6c9aee',
          600: '#5ac1e5',
          700: '#3b7f64',
          800: '#2b5b48',
          900: '#12362a',
        },
        // Accent Colors
        'accent': {
          DEFAULT: '#70c69d',
          50: '#e8f4fd',
          100: '#d1e9fb',
          200: '#a3d3f7',
          300: '#75bdf3',
          400: '#47a7ef',
          500: '#70c69d',
          600: '#5ac1e5',
          700: '#1f5b83',
          800: '#153d58',
          900: '#0a1e2c',
        },
        // Success Colors
        'success': {
          DEFAULT: '#10b981',
          50: '#ecfdf5',
          100: '#d1fae5',
          200: '#a7f3d0',
          300: '#6ee7b7',
          400: '#34d399',
          500: '#10b981',
          600: '#059669',
          700: '#047857',
          800: '#065f46',
          900: '#064e3b',
        },
        // Warning Colors
        'warning': {
          DEFAULT: '#f59e0b',
          50: '#fffbeb',
          100: '#fef3c7',
          200: '#fde68a',
          300: '#fcd34d',
          400: '#fbbf24',
          500: '#f59e0b',
          600: '#d97706',
          700: '#b45309',
          800: '#92400e',
          900: '#78350f',
        },
        // Danger Colors
        'danger': {
          DEFAULT: '#ef4444',
          50: '#fef2f2',
          100: '#fee2e2',
          200: '#fecaca',
          300: '#fca5a5',
          400: '#f87171',
          500: '#ef4444',
          600: '#dc2626',
          700: '#b91c1c',
          800: '#991b1b',
          900: '#7f1d1d',
        },
        // Info Colors
        'info': {
          DEFAULT: '#3b82f6',
          50: '#eff6ff',
          100: '#dbeafe',
          200: '#bfdbfe',
          300: '#93c5fd',
          400: '#60a5fa',
          500: '#3b82f6',
          600: '#2563eb',
          700: '#1d4ed8',
          800: '#1e40af',
          900: '#1e3a8a',
        },
        // Neutral Backgrounds
        'light-bg': '#f8fafc',
        'card-bg': '#ffffff',
        // Coin/Reward Color
        'coin': {
          DEFAULT: '#fbbf24',
          light: '#fef3c7',
          dark: '#f59e0b',
        },
      },
      // Background Gradients
      backgroundImage: {
        'gradient-primary': 'linear-gradient(135deg, #6c9aee 0%, #5ac1e5 100%)',
        'gradient-success': 'linear-gradient(135deg, #10b981 0%, #34d399 100%)',
        'gradient-warning': 'linear-gradient(135deg, #f59e0b 0%, #fbbf24 100%)',
        'gradient-danger': 'linear-gradient(135deg, #ef4444 0%, #f87171 100%)',
        'gradient-accent': 'linear-gradient(135deg, #70c69d 0%, #5ac1e5 100%)',
        'gradient-card': 'linear-gradient(145deg, #ffffff 0%, #f8fafc 100%)',
      },
      // Box Shadows
      boxShadow: {
        'soft': '0 2px 15px 0 rgba(0, 0, 0, 0.05)',
        'medium': '0 4px 25px 0 rgba(0, 0, 0, 0.1)',
        'strong': '0 10px 40px 0 rgba(0, 0, 0, 0.15)',
        'inner-soft': 'inset 0 2px 4px 0 rgba(0, 0, 0, 0.06)',
        'glow-accent': '0 0 20px rgba(52, 152, 219, 0.3)',
        'glow-success': '0 0 20px rgba(16, 185, 129, 0.3)',
      },
      // Animations
      animation: {
        'slide-up': 'slide-up 0.3s ease-out',
        'slide-down': 'slide-down 0.3s ease-out',
        'slide-left': 'slide-left 0.3s ease-out',
        'slide-right': 'slide-right 0.3s ease-out',
        'fade-in': 'fade-in 0.3s ease-out',
        'fade-out': 'fade-out 0.3s ease-out',
        'scale-in': 'scale-in 0.2s ease-out',
        'scale-out': 'scale-out 0.2s ease-out',
        'bounce-soft': 'bounce-soft 0.5s ease-in-out',
        'pulse-soft': 'pulse-soft 2s ease-in-out infinite',
        'spin-slow': 'spin 3s linear infinite',
        'wiggle': 'wiggle 0.5s ease-in-out',
      },
      keyframes: {
        'slide-up': {
          '0%': { transform: 'translateY(100%)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        'slide-down': {
          '0%': { transform: 'translateY(-100%)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        'slide-left': {
          '0%': { transform: 'translateX(100%)', opacity: '0' },
          '100%': { transform: 'translateX(0)', opacity: '1' },
        },
        'slide-right': {
          '0%': { transform: 'translateX(-100%)', opacity: '0' },
          '100%': { transform: 'translateX(0)', opacity: '1' },
        },
        'fade-in': {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        'fade-out': {
          '0%': { opacity: '1' },
          '100%': { opacity: '0' },
        },
        'scale-in': {
          '0%': { transform: 'scale(0.9)', opacity: '0' },
          '100%': { transform: 'scale(1)', opacity: '1' },
        },
        'scale-out': {
          '0%': { transform: 'scale(1)', opacity: '1' },
          '100%': { transform: 'scale(0.9)', opacity: '0' },
        },
        'bounce-soft': {
          '0%, 100%': { transform: 'translateY(0)' },
          '50%': { transform: 'translateY(-10px)' },
        },
        'pulse-soft': {
          '0%, 100%': { opacity: '1' },
          '50%': { opacity: '0.7' },
        },
        'wiggle': {
          '0%, 100%': { transform: 'rotate(0deg)' },
          '25%': { transform: 'rotate(-5deg)' },
          '75%': { transform: 'rotate(5deg)' },
        },
      },
      // Spacing
      spacing: {
        '18': '4.5rem',
        '88': '22rem',
        '112': '28rem',
        '128': '32rem',
      },
      // Border Radius
      borderRadius: {
        '4xl': '2rem',
      },
      // Typography
      fontSize: {
        '2xs': ['0.625rem', { lineHeight: '0.875rem' }],
      },
      // Z-index
      zIndex: {
        '60': '60',
        '70': '70',
        '80': '80',
        '90': '90',
        '100': '100',
      },
      // Transitions
      transitionDuration: {
        '400': '400ms',
      },
    },
  },
  plugins: [],
}