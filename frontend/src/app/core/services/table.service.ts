import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TableService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'https://localhost:7299/api/tables';

  getTables(filters: {
    restaurantId?: string;
    tableTypeId?: number;
    status?: string;
    searchTerm?: string;
    sortBy?: string;
    isAscending?: boolean;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<any> {
    let params = new HttpParams();

    if (filters.restaurantId) params = params.set('restaurantId', filters.restaurantId);
    if (filters.tableTypeId) params = params.set('tableTypeId', filters.tableTypeId.toString());
    if (filters.status) params = params.set('status', filters.status);
    if (filters.searchTerm) params = params.set('searchTerm', filters.searchTerm);
    if (filters.sortBy) params = params.set('sortBy', filters.sortBy);
    if (filters.isAscending !== undefined) params = params.set('isAscending', filters.isAscending.toString());
    if (filters.pageNumber) params = params.set('pageNumber', filters.pageNumber.toString());
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());

    return this.http.get<any>(this.apiUrl, { params });
  }

  createTable(data: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, data);
  }

  updateTable(id: string, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, data);
  }

  deleteTable(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
}
