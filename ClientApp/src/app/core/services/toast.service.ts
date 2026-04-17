import { Injectable, inject } from '@angular/core';
import { MessageService } from 'primeng/api';

type ToastSeverity = 'success' | 'info' | 'warn' | 'error';

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly messageService = inject(MessageService);

  success(summary: string, detail: string, life = 2800): void {
    this.show('success', summary, detail, life);
  }

  info(summary: string, detail: string, life = 2600): void {
    this.show('info', summary, detail, life);
  }

  warn(summary: string, detail: string, life = 3200): void {
    this.show('warn', summary, detail, life);
  }

  error(summary: string, detail: string, life = 3600): void {
    this.show('error', summary, detail, life);
  }

  private show(severity: ToastSeverity, summary: string, detail: string, life: number): void {
    this.messageService.add({
      key: 'global',
      severity,
      summary,
      detail,
      life,
    });
  }
}
