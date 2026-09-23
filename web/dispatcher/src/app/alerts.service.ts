import { Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Observable, Subscription } from 'rxjs';
import { auditTime } from 'rxjs/operators';

export interface Alert {
  deviceId: string;
  temperatureC: number;
  maxTemperatureC: number;
  recordedAt: string;
}

const hubBase = 'http://localhost:5227/hubs/alerts';

@Injectable({ providedIn: 'root' })
export class AlertsService {
  latest = signal<Alert | null>(null);

  private hub?: HubConnection;
  private sub?: Subscription;

  connect(tenantId: string) {
    this.sub?.unsubscribe();
    this.hub?.stop();
    this.latest.set(null);

    const hub = new HubConnectionBuilder()
      .withUrl(hubBase + '?tenantId=' + encodeURIComponent(tenantId))
      .withAutomaticReconnect()
      .build();

    const incoming$ = new Observable<Alert>(observer => {
      hub.on('alert', (msg: Alert) => observer.next(msg));
      return () => hub.off('alert');
    });

    this.sub = incoming$.pipe(auditTime(500)).subscribe(msg => this.latest.set(msg));
    this.hub = hub;
    return hub.start();
  }
}
