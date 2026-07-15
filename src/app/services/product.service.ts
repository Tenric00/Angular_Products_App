import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { Product } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private base = `${environment.apiUrl}/api/products`;

  constructor(private http: HttpClient) {}

  // get all products (both active and inactive) by requesting a large page
  getAll(includeInactive = false): Observable<Product[]> {
    const params = new HttpParams()
      .set('pageNumber', '1')
      .set('pageSize', '1000')
      .set('includeInactive', String(includeInactive));
    return this.http.get<any>(this.base, { params }).pipe(
        // res can be either PagedResult or array, so we normalize it to an array of products
        map((res) => {
        // controller returns either PagedResult or array (depending on pageSize)
        if (Array.isArray(res)) return res as Product[];
        if (res && res.items) return res.items as Product[];
        return [] as Product[];
        })
    );
  }

  // soft-delete via DELETE request
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}