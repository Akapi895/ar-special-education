import { type ComponentPropsWithoutRef } from 'react';

type PageContainerElement = 'div' | 'section' | 'main';

interface PageContainerProps extends ComponentPropsWithoutRef<'div'> {
  as?: PageContainerElement;
  maxWidth?: 'md' | 'lg' | 'xl' | '2xl' | 'full';
  padding?: 'sm' | 'md' | 'lg';
  spacing?: 'sm' | 'md' | 'lg';
  fullHeight?: boolean;
}

const maxWidthStyles = {
  md: 'max-w-4xl',
  lg: 'max-w-5xl',
  xl: 'max-w-7xl',
  '2xl': 'max-w-[88rem]',
  full: 'max-w-full',
};

const paddingStyles = {
  sm: 'px-4 py-4 sm:px-6',
  md: 'px-4 py-6 sm:px-6 lg:px-8',
  lg: 'px-4 py-6 sm:px-6 lg:px-8 xl:px-10',
};

const spacingStyles = {
  sm: 'space-y-4',
  md: 'space-y-6',
  lg: 'space-y-8',
};

const PageContainer = ({
  as: Component = 'div',
  maxWidth = 'xl',
  padding = 'md',
  spacing = 'md',
  fullHeight = false,
  className = '',
  ...props
}: PageContainerProps) => {
  return (
    <Component
      className={`mx-auto w-full ${maxWidthStyles[maxWidth]} ${paddingStyles[padding]} ${spacingStyles[spacing]} ${fullHeight ? 'min-h-screen' : ''} ${className}`}
      {...props}
    />
  );
};

export default PageContainer;