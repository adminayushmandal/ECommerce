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
      {
        path: 'account/profile',
        loadComponent: () =>
          import('./pages/profile-page/profile-page.component').then((m) => m.ProfilePageComponent),
      },
      {
        path: 'manager/dashboard',
        loadComponent: () =>
          import('./pages/store-manager-dashboard-page/store-manager-dashboard-page.component').then(
            (m) => m.StoreManagerDashboardPageComponent
          ),
      },
      {
        path: 'checkout/paypal/return',
        loadComponent: () =>
          import('./pages/paypal-return-page/paypal-return-page.component').then((m) => m.PayPalReturnPageComponent),
      },
      {
        path: 'checkout/paypal/cancel',
        loadComponent: () =>
          import('./pages/paypal-cancel-page/paypal-cancel-page.component').then((m) => m.PayPalCancelPageComponent),
      },
    ],
  },
];
