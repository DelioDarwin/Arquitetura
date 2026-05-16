export interface ApiError {
  title: string;
  status: number;
  errors?: Record<string, string[]>;
  detail?: string;
}
