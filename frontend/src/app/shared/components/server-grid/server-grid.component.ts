import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StatusBadgeComponent } from '../status-badge/status-badge.component';

export interface GridColumn {
  key: string;
  label: string;
  sortable?: boolean;
  isStatus?: boolean;
  isDate?: boolean;
  isTime?: boolean;
  isCurrency?: boolean;
}

@Component({
  selector: 'app-server-grid',
  standalone: true,
  imports: [CommonModule, StatusBadgeComponent],
  templateUrl: './server-grid.component.html'
})
export class ServerGridComponent {
  readonly columns = input.required<GridColumn[]>();
  readonly data = input.required<any[]>();
  readonly totalCount = input.required<number>();
  readonly pageNumber = input<number>(1);
  readonly pageSize = input<number>(10);
  readonly loading = input<boolean>(false);
  readonly actions = input<{ name: string; label: string; condition?: (row: any) => boolean }[]>([]);

  readonly sort = output<{ sortBy: string; isAscending: boolean }>();
  readonly pageChange = output<number>();
  readonly action = output<{ action: string; row: any }>();

  protected currentSortBy = signal<string>('');
  protected currentIsAscending = signal<boolean>(true);

  get totalPages(): number {
    return Math.ceil(this.totalCount() / this.pageSize());
  }

  get pageNumbers(): number[] {
    const pages = [];
    for (let i = 1; i <= this.totalPages; i++) {
      pages.push(i);
    }
    return pages;
  }

  onSort(col: GridColumn): void {
    if (!col.sortable) return;
    
    if (this.currentSortBy() === col.key) {
      this.currentIsAscending.set(!this.currentIsAscending());
    } else {
      this.currentSortBy.set(col.key);
      this.currentIsAscending.set(true);
    }
    this.sort.emit({ sortBy: this.currentSortBy(), isAscending: this.currentIsAscending() });
  }

  onPageChange(page: number): void {
    this.pageChange.emit(page);
  }

  onAction(actionName: string, row: any): void {
    this.action.emit({ action: actionName, row });
  }

  showActionButton(act: any, row: any): boolean {
    if (act.condition) {
      return act.condition(row);
    }
    return true;
  }

  getButtonClass(act: any): string {
    const name = act.name.toLowerCase();
    if (['edit', 'confirm', 'check-in', 'complete'].includes(name) || name.includes('checkin')) {
      return 'bg-primary hover:bg-primary-hover text-slate-950 font-bold shadow-md shadow-primary/20 hover:shadow-lg';
    }
    if (['delete', 'cancel'].includes(name)) {
      return 'bg-red-500 hover:bg-red-650 text-white font-bold shadow-md shadow-red-500/10 hover:shadow-lg';
    }
    return 'border border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800';
  }
}
