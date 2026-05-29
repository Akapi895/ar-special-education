/**
 * Route Suspense Wrapper
 * Provides loading fallback for lazy-loaded routes
 */

import { Suspense } from 'react';
import type { ReactNode } from 'react';
import Loading from '../ui/Loading';

interface RouteSuspenseProps {
    children: ReactNode;
    fallback?: ReactNode;
}

export const RouteSuspense = ({ children, fallback }: RouteSuspenseProps) => {
    const defaultFallback = (
        <div className="min-h-screen flex items-center justify-center bg-(--app-bg) transition-colors duration-300">
            <Loading text="Đang tải trang..." size="lg" />
        </div>
    );

    return (
        <Suspense fallback={fallback || defaultFallback}>
            {children}
        </Suspense>
    );
};

export default RouteSuspense;
