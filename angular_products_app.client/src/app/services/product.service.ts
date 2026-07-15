import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, throwError, of } from 'rxjs';
import { environment } from '../../environments/environment';
import { Product } from '../models/product.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  // If your server is hosted on a different origin and you intentionally need an absolute apiUrl,
  // ensure the server is listening on that port and CORS is allowed — but for the typical integrated dev setup using the same host,
  // the relative path is more reliable.
  private base = `${environment.apiUrl}/api/products`;
  // Use relative path so client requests go to same origin the app was served from.
  /*private base = '/api/products'; */

  constructor(private http: HttpClient) {}

  getPaged(pageNumber = 1, pageSize = 5, nameFilter?: string,
           includeInactive = false, onlyInactive = false): Observable<PagedResult<Product>> {
    let params = new HttpParams()
      .set('pageNumber', String(pageNumber))
      .set('pageSize', String(pageSize))
      .set('includeInactive', String(includeInactive))
      .set('onlyInactive', String(onlyInactive));

    if (nameFilter) {
      params = params.set('nameFilter', nameFilter);
    }

    return this.http.get<PagedResult<Product>>(this.base, { params }).pipe(
      catchError(err => throwError(() => err))
    );
  }

  // Returns a mock paged result as an Observable for tests/demo
  getPagedMockData(): Observable<PagedResult<Product>> {
    const mock: PagedResult<Product> = {
      items: [
        { id: 1, name: 'Acme Widget', image: null, description: 'Small widget', price: 9.99, active: true, inActiveDate: null },
        { id: 2, name: 'Acme Mid Widget', image: null, description: 'Mid widget', price: 14.99, active: true, inActiveDate: null },
        { id: 3, name: 'Acme Pro Widget', image: null, description: 'Pro widget with extras', price: 19.99, active: true, inActiveDate: null }
      ],
      totalCount: 8,
      pageNumber: 1,
      pageSize: 5
    };

    // Return the mock data as an Observable
    return of(mock);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`).pipe(
      catchError(err => throwError(() => err))
    );
  }
}
