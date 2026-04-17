import { Routes } from '@angular/router';

export const identityRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layouts/auth-shell/auth-shell.component').then((m) => m.AuthShellComponent),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'login',
      },
      {
        path: 'login',
        loadComponent: () =>
          import('./pages/login-page/login-page.component').then((m) => m.LoginPageComponent),
      },
      {
        path: 'signup',
        loadComponent: () =>
          import('./pages/signup-page/signup-page.component').then((m) => m.SignupPageComponent),
      },
    ],
  },
];
