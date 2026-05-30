export interface PurchaseTransaction {
  id: string;
  description: string;
  transactionDate: string; // ISO date string yyyy-MM-dd
  amountUsd: number;
  createdAt: string;
}

export interface CreateTransactionRequest {
  description: string;
  transactionDate: string;
  amountUsd: number;
}

export interface ConvertedTransaction {
  id: string;
  description: string;
  transactionDate: string;
  originalAmountUsd: number;
  targetCurrency: string;
  exchangeRate: number;
  convertedAmount: number;
}

export interface ApiError {
  title: string;
  status: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
