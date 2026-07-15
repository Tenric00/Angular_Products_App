export interface Product {
  id: number;
  name: string;
  image?: string | null;
  description?: string | null;
  price: number;
  active: boolean;
  inActiveDate?: string | null;
}