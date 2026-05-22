# HTTP k6 Script Template

```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Trend, Rate } from 'k6/metrics';

export const options = {
  scenarios: {
    test_scenario: {
      exec: 'runScenario',
      executor: 'constant-vus',
      vus: __ENV.VUS ? parseInt(__ENV.VUS) : 10,
      duration: __ENV.DURATION ? __ENV.DURATION : '60s',
      gracefulStop: __ENV.GRACEFUL_STOP ? __ENV.GRACEFUL_STOP : '10s'
    }
  },
  thresholds: {
    http_req_duration: ['p(95) < 500'],
    http_req_failed: ['rate<0.1'],
    scenario_success_rate: ['rate>0.95']
  }
};

const webappUrl = __ENV.WEBAPP_URL || 'https://localhost:7175';
const successRate = new Rate('scenario_success_rate');
const responseTime = new Trend('scenario_response_time');
const requestCount = new Counter('scenario_requests_total');

export function runScenario() {
  requestCount.add(1);

  const res = http.get(`${webappUrl}/{endpoint}`, {
    headers: {
      correlationId: crypto.randomUUID(),
      Accept: 'application/json',
      CacheEnabled: 'false'
    }
  });

  responseTime.add(res.timings.duration);
  const ok = check(res, { 'status is 200': r => r.status === 200 });
  successRate.add(ok);
  sleep(1);
}
```
