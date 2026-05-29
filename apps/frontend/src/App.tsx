import { AppRouter } from './routes/AppRouter';

function App() {
  return (
    <div id="main-content" tabIndex={-1} className="w-full min-h-screen bg-[var(--app-bg)] text-[var(--app-text)] transition-colors duration-300">
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-[100] focus:rounded-xl focus:bg-[var(--app-surface-strong)] focus:px-4 focus:py-2 focus:text-sm focus:font-semibold focus:text-primary-700 focus:shadow-lg"
      >
        Bỏ qua đến nội dung chính
      </a>
      <AppRouter />
    </div>
  );
}

export default App;