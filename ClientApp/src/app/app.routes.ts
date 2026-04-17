import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./features/store/store.routes').then((m) => m.storeRoutes),
  },
  {
    path: 'identity',
    loadChildren: () => import('./features/identity/identity.routes').then((m) => m.identityRoutes),
  },
  {
    path: '**',
    redirectTo: '',
  },
];

