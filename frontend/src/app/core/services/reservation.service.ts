import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ReservationService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'https://localhost:7299/api/reservations';

  getReservations(filters: {
    restaurantId?: string;
    status?: string;
    reservationDate?: string;
    searchTerm?: string;
    sortBy?: string;
    isAscending?: boolean;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<any> {
    let params = new HttpParams();

    if (filters.restaurantId) params = params.set('restaurantId', filters.restaurantId);
    if (filters.status) params = params.set('status', filters.status);
    if (filters.reservationDate) params = params.set('reservationDate', filters.reservationDate);
    if (filters.searchTerm) params = params.set('searchTerm', filters.searchTerm);
    if (filters.sortBy) params = params.set('sortBy', filters.sortBy);
    if (filters.isAscending !== undefined) params = params.set('isAscending', filters.isAscending.toString());
    if (filters.pageNumber) params = params.set('pageNumber', filters.pageNumber.toString());
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());

    return this.http.get<any>(this.apiUrl, { params });
  }

  getMyReservations(filters: {
    searchTerm?: string;
    sortBy?: string;
    isAscending?: boolean;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<any> {
    let params = new HttpParams();

    if (filters.searchTerm) params = params.set('searchTerm', filters.searchTerm);
    if (filters.sortBy) params = params.set('sortBy', filters.sortBy);
    if (filters.isAscending !== undefined) params = params.set('isAscending', filters.isAscending.toString());
    if (filters.pageNumber) params = params.set('pageNumber', filters.pageNumber.toString());
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());

    return this.http.get<any>(`${this.apiUrl}/my`, { params });
  }

  requestReservation(data: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, data);
  }

  confirmReservation(id: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/confirm`, {});
  }

  checkInReservation(id: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/checkin`, {});
  }

  completeReservation(id: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/complete`, {});
  }

  cancelReservation(id: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/cancel`, {});
  }
}
