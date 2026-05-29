import DashboardPage from '../pages/parents/DashboardPage';
import ExerciseLibraryPage from '../pages/parents/ExerciseLibraryPage';
import JournalPage from '../pages/parents/JournalPage';
import MethodsPage from '../pages/parents/MethodsPage';
import SettingsPage from '../pages/parents/SettingsPage';
import { type ComponentType } from 'react';

interface ParentRoute {
  path: string;
  Component: ComponentType;
}

export const parentRoutes: ParentRoute[] = [
  {
    path: 'dashboard',
    Component: DashboardPage,
  },
  {
    path: 'exercises',
    Component: ExerciseLibraryPage,
  },
  {
    path: 'journal',
    Component: JournalPage,
  },
  {
    path: 'methods',
    Component: MethodsPage,
  },
  {
    path: 'settings',
    Component: SettingsPage,
  },
];