import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ProductService } from './product.service';
import { environment } from '../../environments/environment';

describe('ProductService', () => {
  let service: ProductService;
  let httpMock: HttpTestingController;
  const base = `${environment.apiUrl}/api/products`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ProductService]
    });
    service = TestBed.inject(ProductService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should request paged products', (done) => {
    service.getPaged(2, 5, 'foo', false, false).subscribe(res => {
      expect(res.pageNumber).toBe(2);
      expect(res.pageSize).toBe(5);
      done();
    });

    const req = httpMock.expectOne(r => r.method === 'GET' && r.url === base);
    expect(req.request.params.get('pageNumber')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('5');
    req.flush({ items: [], totalCount: 0, pageNumber: 2, pageSize: 5 });
  });

  it('should call delete endpoint', (done) => {
    service.delete(42).subscribe(() => done());
    const req = httpMock.expectOne(`${base}/42`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should return paged product items', (done) => {
    const mockProduct = {
      id: 7,
      name: 'Test Product',
      image: null,
      description: 'Desc',
      price: 12.5,
      active: true,
      inActiveDate: null
    };

    service.getPaged(1, 5).subscribe(res => {
      expect(res.pageNumber).toBe(1);
      expect(res.pageSize).toBe(5);
      expect(res.totalCount).toBe(1);
      expect(res.items.length).toBe(1);
      expect(res.items[0].id).toBe(mockProduct.id);
      expect(res.items[0].name).toBe(mockProduct.name);
      done();
    });

    const req = httpMock.expectOne(r => r.method === 'GET' && r.url === base);
    req.flush({ items: [mockProduct], totalCount: 1, pageNumber: 1, pageSize: 5 });
  });

  it('should soft delete product and reflect in inactive paged results', (done) => {
    const idToDelete = 7;

    // Call delete and flush the DELETE request
    service.delete(idToDelete).subscribe(() => {
      // After successful delete, request only inactive products
      service.getPaged(1, 5, undefined, true, true).subscribe(res => {
        expect(res.items.length).toBe(1);
        const p = res.items[0];
        expect(p.id).toBe(idToDelete);
        expect(p.active).toBeFalse();
        done();
      });

      const reqGet = httpMock.expectOne(r => r.method === 'GET' && r.url === base);
      expect(reqGet.request.params.get('includeInactive')).toBe('true');
      expect(reqGet.request.params.get('onlyInactive')).toBe('true');
      reqGet.flush({
        items: [
          {
            id: idToDelete,
            name: 'Deleted Product',
            image: null,
            description: 'Deleted',
            price: 0,
            active: false,
            inActiveDate: new Date().toISOString()
          }
        ],
        totalCount: 1,
        pageNumber: 1,
        pageSize: 5
      });
    });

    const reqDelete = httpMock.expectOne(`${base}/${idToDelete}`);
    expect(reqDelete.request.method).toBe('DELETE');
    reqDelete.flush(null);
  });
});
