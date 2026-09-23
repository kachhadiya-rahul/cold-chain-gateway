import { JsonPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { AlertsService } from './alerts.service';

@Component({
  selector: 'app-root',
  imports: [JsonPipe],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  alerts = inject(AlertsService);

  connect(tenantId: string) {
    this.alerts.connect(tenantId);
  }
}
