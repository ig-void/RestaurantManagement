import { Component, input, output } from '@angular/core';

export interface DropdownFilter {
  key: string;
  label: string;
  options: { value: string | number; label: string }[];
}

@Component({
  selector: 'app-search-filter',
  standalone: true,
  templateUrl: './search-filter.component.html'
})
export class SearchFilterComponent {
  readonly placeholder = input<string>('Search...');
  readonly dropdowns = input<DropdownFilter[]>([]);

  readonly search = output<string>();
  readonly filterChange = output<{ key: string; value: any }>();

  protected searchVal = '';
  private searchTimeout: any;

  onSearchChange(val: string): void {
    this.searchVal = val;
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
    this.searchTimeout = setTimeout(() => {
      this.search.emit(val.trim());
    }, 400);
  }

  onFilterChange(key: string, value: any): void {
    this.filterChange.emit({ key, value: value === '' ? null : value });
  }
}
