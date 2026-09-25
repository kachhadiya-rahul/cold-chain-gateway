import http from 'k6/http';
import { check } from 'k6';

const url = 'http://localhost:5227/api/v1/telemetry';

export const options = {
  vus: 500,
  duration: '12s',
  thresholds: {
    http_req_duration: ['p(95)<100'],
    checks: ['rate==1'],
  },
};

http.setResponseCallback(http.expectedStatuses(202, 503));

export default function () {
  const tenant = Math.random() < 0.5 ? 'pharma' : 'frozen';
  const hot = Math.random() < 0.5;
  let temp;
  if (tenant === 'pharma') temp = hot ? 5.6 : 3.2;
  else temp = hot ? 3.4 : 1.1;

  const res = http.post(
    url,
    JSON.stringify({
      tenantId: tenant,
      deviceId: `R-${__VU}-${__ITER}`,
      latitude: 43.65,
      longitude: -79.38,
      temperatureC: temp,
      humidity: 40,
      recordedAt: new Date().toISOString(),
    }),
    { headers: { 'Content-Type': 'application/json' } },
  );

  check(res, {
    '202 or 503': (r) => r.status === 202 || r.status === 503,
  });
}
