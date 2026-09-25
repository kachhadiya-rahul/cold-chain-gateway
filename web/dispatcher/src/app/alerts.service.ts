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
  alerts = signal<Alert[]>([]);

  private hub?: HubConnection;
  private sub?: Subscription;

  connect(tenantId: string) {
    this.sub?.unsubscribe();
    this.hub?.stop();
    this.alerts.set([]);

    const byDevice = new Map<string, Alert>();

    const hub = new HubConnectionBuilder()
      .withUrl(hubBase + '?tenantId=' + encodeURIComponent(tenantId))
      .withAutomaticReconnect()
      .build();

    const incoming$ = new Observable<Alert>(observer => {
      hub.on('alert', (msg: Alert) => {
        byDevice.set(msg.deviceId, msg);
        observer.next(msg);
      });
      return () => hub.off('alert');
    });

    // board ticks on the latest in each 500ms window, not every ping
    this.sub = incoming$.pipe(auditTime(500)).subscribe(() => {
      this.alerts.set([...byDevice.values()]);
    });

    this.hub = hub;
    return hub.start();
  }
}
