import { ChevronRight, Home } from 'lucide-react';
import { Link, useLocation } from 'react-router-dom';
import { useMemo } from 'react';

const routeLabels: Record<string, string> = {
  dashboard: 'Dashboard',
  exercises: 'Exercise Library',
  journal: 'Emotion Journal',
  methods: 'CPA Guidance',
  settings: 'Settings',
};

const getLabel = (segment: string) => routeLabels[segment] ?? segment.charAt(0).toUpperCase() + segment.slice(1);

const Breadcrumbs = () => {
  const location = useLocation();

  const breadcrumbs = useMemo(() => {
    const segments = location.pathname.split('/').filter(Boolean);

    if (segments.length === 0) {
      return [];
    }

    return segments.map((segment, index) => {
      const href = `/${segments.slice(0, index + 1).join('/')}`;
      return {
        label: getLabel(segment),
        href,
        isLast: index === segments.length - 1,
      };
    });
  }, [location.pathname]);

  if (breadcrumbs.length === 0) {
    return null;
  }

  return (
    <nav aria-label="Breadcrumb" className="mb-5">
      <ol className="flex flex-wrap items-center gap-2 text-sm text-(--app-text-muted)">
        <li className="flex items-center gap-2">
          <Link
            to="/parent/dashboard"
            className="inline-flex items-center gap-1.5 font-medium text-(--app-text) transition-colors hover:text-primary"
          >
            <Home className="h-4 w-4" />
            Parent Home
          </Link>
        </li>
        {breadcrumbs.map((breadcrumb) => (
          <li key={breadcrumb.href} className="flex items-center gap-2">
            <ChevronRight className="h-4 w-4 shrink-0 text-(--app-text-muted)" />
            {breadcrumb.isLast ? (
              <span className="font-semibold text-(--app-text)">{breadcrumb.label}</span>
            ) : (
              <Link
                to={breadcrumb.href}
                className="transition-colors hover:text-primary"
              >
                {breadcrumb.label}
              </Link>
            )}
          </li>
        ))}
      </ol>
    </nav>
  );
};

export default Breadcrumbs;
