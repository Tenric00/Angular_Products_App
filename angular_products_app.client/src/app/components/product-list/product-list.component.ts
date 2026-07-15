import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { Product } from '../../models/product.model';
import { ProductService } from '../../services/product.service';
import { ToastService } from '../../services/toast.service';
import { PagedResult } from '../../models/paged-result.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit, OnDestroy {
  active: Product[] = [];
  inactive: Product[] = [];
  activePaged?: PagedResult<Product>;
  inactivePaged?: PagedResult<Product>;
  loading = false;
  pageSizeOptions = [5, 10, 20];
  activePage = 1;
  inactivePage = 1;
  pageSize = 5;
  view: 'active' | 'inactive' = 'active';
  nameFilter = '';
  Math = Math; // Expose Math to template for Any Math function (ceiling in this case) in the template

  // Use a Subject to manage unsubscription and prevent memory leaks
  private destroy$ = new Subject<void>();

  constructor(
    private productService: ProductService,
    private toast: ToastService,
    private cd: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadActive();
    this.loadInactive();
  }

  loadActive(): void {
    this.loading = true;
    this.productService.getPaged(this.activePage, this.pageSize, this.nameFilter, false, false)
      .pipe(takeUntil(this.destroy$)) // Automatically unsubscribe when destroy$ emits
      .subscribe({
        next: (res) => {
          this.activePaged = res;
          this.active = res.items;
          this.loading = false;

          // Ensure Angular runs change detection immediately so the template updates
          // when the request completes.
          try {
            this.cd.detectChanges();
          } catch {
            // detectChanges can throw if called at certain times (rare). In that case,
            // mark for check as a safe fallback.
            this.cd.markForCheck();
          }
        },
        error: (err) => {
          console.error('Failed loading products', err);
          const message = (err?.error && err.error.message) ? err.error.message : (err.message || 'Unknown network error');
          this.toast.error(`Failed to load products: ${message}. Ensure backend is running and CORS/proxy is configured.`);
          this.loading = false;

          // Also ensure UI updates to reflect loading=false and error state
          try {
            this.cd.detectChanges();
          } catch {
            this.cd.markForCheck();
          }
        }
      });
  }

  loadInactive(): void {
    this.productService.getPaged(this.inactivePage, this.pageSize, this.nameFilter, true, true)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.inactivePaged = res;
          this.inactive = res.items;

          // Ensure template updates for inactive list too
          try {
            this.cd.detectChanges();
          } catch {
            this.cd.markForCheck();
          }
        },
        error: (err) => {
            console.error('Failed loading products', err);
            const message = (err?.error && err.error.message) ? err.error.message : (err.message || 'Unknown network error');
            this.toast.error(`Failed to load inactive products: ${message}. Ensure backend is running and CORS/proxy is configured.`);
            this.loading = false;

            try {
              this.cd.detectChanges();
            } catch {
              this.cd.markForCheck();
            }
          }
      });
  }

  onDelete(product: Product): void {
    if (!confirm(`Mark "${product.name}" as inactive?`)) return;
    this.productService.delete(product.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toast.success(`"${product.name}" marked inactive.`);
          // refresh current pages
          this.loadActive();
          this.loadInactive();
        },
        error: (err) => {
          console.error('Delete failed', err);
          const message = err?.message ?? 'Unknown network error';
          this.toast.error(`Unable to mark product inactive: ${message}`);
        }
      });
  }

  changeActivePage(page: number) {
    this.activePage = page;
    this.loadActive();
  }

  changeInactivePage(page: number) {
    this.inactivePage = page;
    this.loadInactive();
  }

  changePageSize(size: number) {
    this.pageSize = Number(size);
    this.activePage = 1;
    this.inactivePage = 1;
    this.loadActive();
    this.loadInactive();
  }

  // Unsubscribe from all subscriptions to prevent memory leaks
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
