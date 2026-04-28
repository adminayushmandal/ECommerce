import { Injectable, computed, signal } from '@angular/core';

import { DemoLocationKey, ShopperLocation } from '../models/store.models';

const storageKey = 'ecommerce.shopper-location';
const demoLocations: Record<DemoLocationKey, ShopperLocation> = {
  ludhiana: {
    latitude: 30.900965,
    longitude: 75.857275,
    label: 'Ludhiana store demo',
    source: 'demo',
  },
  dibrugarh: {
    latitude: 27.472833,
    longitude: 94.911964,
    label: 'Dibrugarh store demo',
    source: 'demo',
  },
  shillong: {
    latitude: 25.578773,
    longitude: 91.893254,
    label: 'Shillong store demo',
    source: 'demo',
  },
};

@Injectable({ providedIn: 'root' })
export class ShopperLocationService {
  private readonly locationState = signal<ShopperLocation | null>(this.restoreLocation());

  readonly location = computed(() => this.locationState());
  readonly hasLocation = computed(() => this.location() !== null);

  async useBrowserLocation(): Promise<boolean> {
    if (typeof navigator === 'undefined' || !navigator.geolocation) {
      return false;
    }

    return new Promise<boolean>((resolve) => {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          this.setLocation({
            latitude: position.coords.latitude,
            longitude: position.coords.longitude,
            label: 'Current location',
            source: 'browser',
          });

          resolve(true);
        },
        () => resolve(false),
        {
          enableHighAccuracy: false,
          timeout: 10000,
          maximumAge: 300000,
        }
      );
    });
  }

  useLudhianaDemoLocation(): void {
    this.useDemoLocation('ludhiana');
  }

  useDemoLocation(location: DemoLocationKey): void {
    this.setLocation(demoLocations[location]);
  }

  clear(): void {
    this.locationState.set(null);
    this.persist();
  }

  private setLocation(location: ShopperLocation): void {
    this.locationState.set(location);
    this.persist();
  }

  private restoreLocation(): ShopperLocation | null {
    if (typeof window === 'undefined' || !window.localStorage) {
      return null;
    }

    try {
      const rawValue = window.localStorage.getItem(storageKey);
      if (!rawValue) {
        return null;
      }

      const candidate = JSON.parse(rawValue) as Partial<ShopperLocation>;

      if (
        typeof candidate.latitude !== 'number' ||
        typeof candidate.longitude !== 'number' ||
        typeof candidate.label !== 'string' ||
        (candidate.source !== 'browser' && candidate.source !== 'demo')
      ) {
        return null;
      }

      return {
        latitude: candidate.latitude,
        longitude: candidate.longitude,
        label: candidate.label,
        source: candidate.source,
      };
    } catch {
      return null;
    }
  }

  private persist(): void {
    if (typeof window === 'undefined' || !window.localStorage) {
      return;
    }

    const location = this.locationState();

    if (!location) {
      window.localStorage.removeItem(storageKey);
      return;
    }

    window.localStorage.setItem(storageKey, JSON.stringify(location));
  }
}
