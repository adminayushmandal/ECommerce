export interface StreamProductDetailRequest {
  userQuery: string;
  productId?: string | null;
}

export interface EnmaStreamChunk {
  type: 'delta' | 'completed';
  content?: string | null;
}
