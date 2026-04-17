import { Routes } from '@angular/router';

export const storeRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layouts/store-shell/store-shell.component').then((m) => m.StoreShellComponent),
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./pages/storefront-page/storefront-page.component').then((m) => m.StorefrontPageComponent),
      },
      {
        path: 'products/:slug',
        loadComponent: () =>
          import('./pages/product-detail-page/product-detail-page.component').then((m) => m.ProductDetailPageComponent),
      },
    ],
  },
];
