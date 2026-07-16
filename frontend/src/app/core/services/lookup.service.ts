import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LookupService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'https://localhost:7299/api/lookups';

  getCuisines(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/cuisines`);
  }

  getTableTypes(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/table-types`);
  }

  getRestaurants(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/restaurants`);
  }
}
