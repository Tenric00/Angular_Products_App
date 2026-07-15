import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject, takeUntil } from  'rxjs';
import { Product } from '../../models/product.model';
import { ProductService } from '../../services/product.service';

@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit, OnDestroy {
  active: Product[] = [];
  inactive: Product[] = [];
  loading = false;
  private destroy$ = new Subject<void>();

  constructor(private svc: ProductService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    // request all and separate active/inactive
    this.svc.getAll(true)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (list) => {
          this.active = list.filter(p => p.active);
          this.inactive = list.filter(p => !p.active);
          this.loading = false;
        },
        error: () => {
          this.loading = false;
        }
      });
  }

  onDelete(product: Product): void {
    if (!confirm(`Mark "${product.name}" as inactive?`)) return;
    this.svc.delete(product.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          // move to inactive list locally
          this.active = this.active.filter(p => p.id !== product.id);
          const updated = { ...product, active: false, inActiveDate: new Date().toISOString() };
          this.inactive = [updated, ...this.inactive];
        },
        error: (err) => {
          console.error('Delete failed', err);
          alert('Unable to delete product.');
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}