import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

import { EnmaStreamChunk, StreamProductDetailRequest } from '../models/enma.models';

interface StreamProductDetailOptions {
  signal?: AbortSignal;
  onChunk: (content: string) => void;
}

@Injectable({ providedIn: 'root' })
export class EnmaApiService {
  private readonly hubUrl = '/hubs/enma';
  private hubConnection?: signalR.HubConnection;
  private connectingPromise?: Promise<signalR.HubConnection>;

  async streamProductDetail(
    request: StreamProductDetailRequest,
    options: StreamProductDetailOptions,
  ): Promise<string> {
    const connection = await this.getConnection();

    return await new Promise<string>((resolve, reject) => {
      let fullResponse = '';
      let completed = false;

      const streamResult = connection.stream<EnmaStreamChunk>(
        'StreamProductDetail',
        request.userQuery,
        request.productId ?? null,
      );

      const cleanup = () => {
        options.signal?.removeEventListener('abort', handleAbort);
      };

      const resolveWithCurrentContent = () => {
        cleanup();
        resolve(fullResponse);
      };

      const subscription = streamResult.subscribe({
        next: (chunk) => {
          const normalizedChunk = this.normalizeChunk(chunk);

          if (!normalizedChunk) {
            return;
          }

          if (normalizedChunk.type === 'delta' && normalizedChunk.content) {
            fullResponse += normalizedChunk.content;
            options.onChunk(normalizedChunk.content);
          }

          if (normalizedChunk.type === 'completed' && !completed) {
            completed = true;
            resolveWithCurrentContent();
          }
        },
        complete: () => {
          if (!completed) {
            resolveWithCurrentContent();
          }
        },
        error: (error) => {
          cleanup();
          reject(error instanceof Error ? error : new Error('Unable to get a response from Enma right now.'));
        },
      });

      const handleAbort = () => {
        subscription.dispose();
        resolveWithCurrentContent();
      };

      if (options.signal?.aborted) {
        handleAbort();
        return;
      }

      options.signal?.addEventListener('abort', handleAbort, { once: true });
    });
  }

  private normalizeChunk(chunk: EnmaStreamChunk | Record<string, unknown> | null | undefined): EnmaStreamChunk | null {
    if (!chunk) {
      return null;
    }

    const chunkRecord = chunk as Record<string, unknown>;
    const type = (chunkRecord['type'] ?? chunkRecord['Type']) as string | undefined;
    const content = (chunkRecord['content'] ?? chunkRecord['Content']) as string | null | undefined;

    if (!type) {
      return null;
    }

    return {
      type: type as EnmaStreamChunk['type'],
      content,
    };
  }

  private async getConnection(): Promise<signalR.HubConnection> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return this.hubConnection;
    }

    if (this.connectingPromise) {
      return this.connectingPromise;
    }

    const connection =
      this.hubConnection ??
      new signalR.HubConnectionBuilder()
        .withUrl(this.hubUrl)
        .withAutomaticReconnect()
        .build();

    this.hubConnection = connection;
    this.connectingPromise = connection
      .start()
      .then(() => connection)
      .finally(() => {
        this.connectingPromise = undefined;
      });

    return this.connectingPromise;
  }
}
