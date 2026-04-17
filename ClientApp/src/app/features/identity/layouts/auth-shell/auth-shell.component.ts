import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-auth-shell',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './auth-shell.component.html',
  styleUrl: './auth-shell.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthShellComponent {
  protected readonly highlights = [
    'Fast account creation with clear inline validation',
    'Cookie-based sign-in flow built for the ASP.NET Identity backend',
    'Responsive layout designed for mobile checkout-first journeys',
  ];
}
