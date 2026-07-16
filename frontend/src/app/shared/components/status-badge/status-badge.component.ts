import { Component, input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  templateUrl: './status-badge.component.html'
})
export class StatusBadgeComponent {
  readonly status = input.required<string>();

  get badgeClass(): string {
    const s = this.status().toLowerCase();
    if (['active', 'confirmed', 'completed', 'checked-in', 'available'].includes(s)) {
      return 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/30';
    }
    if (['maintenance', 'pending', 'reserved'].includes(s)) {
      return 'bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-500/30';
    }
    if (['inactive', 'cancelled', 'occupied'].includes(s)) {
      return 'bg-rose-500/10 text-rose-600 dark:text-rose-400 border-rose-500/30';
    }
    return 'bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/30';
  }
}
