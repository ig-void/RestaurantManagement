import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableService } from '../../../core/services/table.service';
import { LookupService } from '../../../core/services/lookup.service';
import { TableFormComponent } from './table-form/table-form.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-tables-admin',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    TableFormComponent, 
    ConfirmDialogComponent, 
    PaginationComponent, 
    StatusBadgeComponent
  ],
  templateUrl: './tables-admin.component.html'
})
export class TablesAdminComponent implements OnInit {
  private readonly tableService = inject(TableService);
  private readonly lookupService = inject(LookupService);

  // States
  protected readonly tables = signal<any[]>([]);
  protected readonly loading = signal<boolean>(true);
  protected readonly errorMessage = signal<string | null>(null);

  // Search, Filters & Pagination
  protected searchTerm = '';
  protected filterRestaurant: string | null = null;
  protected filterTableType: number | null = null;
  protected filterStatus: string | null = null;
  protected readonly pageNumber = signal<number>(1);
  protected totalCount = 0;
  protected readonly pageSize = 10;
  protected sortBy = 'tablenumber';
  protected isAscending = true;

  // Lookups
  protected restaurants: any[] = [];
  protected tableTypes: any[] = [];

  // Modal form states
  protected readonly showFormModal = signal<boolean>(false);
  protected isEditMode = false;
  protected readonly editingData = signal<any>(null);
  protected readonly formLoading = signal<boolean>(false);

  // Delete dialog states
  protected readonly showDeleteDialog = signal<boolean>(false);
  protected readonly deletingTable = signal<any | null>(null);
  private searchTimeout: any;

  ngOnInit(): void {
    this.loadLookups();
    this.loadTables();
  }

  loadLookups(): void {
    // Load Restaurants
    this.lookupService.getRestaurants().subscribe(res => {
      this.restaurants = res;
    });

    // Load Table Types
    this.lookupService.getTableTypes().subscribe(types => {
      this.tableTypes = types;
    });
  }

  loadTables(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.tableService.getTables({
      searchTerm: this.searchTerm,
      restaurantId: this.filterRestaurant || undefined,
      tableTypeId: this.filterTableType || undefined,
      status: this.filterStatus || undefined,
      sortBy: this.sortBy,
      isAscending: this.isAscending,
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize
    }).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.tables.set(res.items);
        this.totalCount = res.totalCount;
      },
      error: () => this.loading.set(false)
    });
  }

  onSearchChange(term: string): void {
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
    this.searchTimeout = setTimeout(() => {
      this.searchTerm = term;
      this.pageNumber.set(1);
      this.loadTables();
    }, 400);
  }

  protected toggleSort(column: string): void {
    if (this.sortBy === column) {
      this.isAscending = !this.isAscending;
    } else {
      this.sortBy = column;
      this.isAscending = true;
    }
    this.loadTables();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadTables();
  }

  openAddForm(): void {
    this.isEditMode = false;
    this.editingData.set(null);
    this.showFormModal.set(true);
  }

  openEditForm(row: any): void {
    this.isEditMode = true;
    this.editingData.set(row);
    this.showFormModal.set(true);
  }

  closeFormModal(): void {
    this.showFormModal.set(false);
    this.editingData.set(null);
  }

  onSubmitForm(formData: any): void {
    this.formLoading.set(true);
    this.errorMessage.set(null);

    const request = this.isEditMode
      ? this.tableService.updateTable(this.editingData().id, { id: this.editingData().id, ...formData })
      : this.tableService.createTable(formData);

    request.subscribe({
      next: (res) => {
        this.formLoading.set(false);
        if (res.success) {
          this.closeFormModal();
          this.loadTables();
        } else {
          this.errorMessage.set(res.message);
        }
      },
      error: (err) => {
        this.formLoading.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to submit form.');
      }
    });
  }

  // Delete Actions
  openDeleteConfirmation(row: any): void {
    this.deletingTable.set(row);
    this.showDeleteDialog.set(true);
  }

  handleConfirmDelete(confirmed: boolean): void {
    const t = this.deletingTable();
    this.showDeleteDialog.set(false);
    this.deletingTable.set(null);

    if (confirmed && t) {
      this.loading.set(true);
      this.tableService.deleteTable(t.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.loadTables();
          } else {
            this.loading.set(false);
            this.errorMessage.set(res.message);
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.errorMessage.set(err.error?.message || 'Failed to delete table.');
        }
      });
    }
  }
}
