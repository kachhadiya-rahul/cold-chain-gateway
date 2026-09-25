import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AlertsService } from './alerts.service';

const ingest = 'http://localhost:5227/api/v1/telemetry';

@Component({
  selector: 'app-root',
  imports: [],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  alerts = inject(AlertsService);
  tenant = 'pharma';

  constructor() {
    const q = new URLSearchParams(location.search).get('tenant');
    if (q === 'frozen' || q === 'pharma') this.tenant = q;
    this.alerts.connect(this.tenant);
  }

  pick(id: 'pharma' | 'frozen') {
    if (id === this.tenant) return;
    this.tenant = id;
    this.alerts.connect(id);
  }

  burst() {
    const over = this.tenant === 'pharma' ? 5.4 : 3.1;
    for (let i = 0; i < 50; i++) {
      fetch(ingest, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          tenantId: this.tenant,
          deviceId: 'R-' + (100 + i),
          latitude: 43.65,
          longitude: -79.38,
          temperatureC: over,
          humidity: 41,
          recordedAt: new Date().toISOString(),
        }),
      });
    }
  }
}
