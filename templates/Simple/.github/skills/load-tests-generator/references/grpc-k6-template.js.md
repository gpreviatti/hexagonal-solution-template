# gRPC k6 Script Template

```javascript
import grpc from 'k6/net/grpc';
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
    grpc_req_duration: ['p(95) < 100'],
    scenario_success_rate: ['rate>0.95']
  }
};

const webappUrl = __ENV.WEBAPP_GRPC_URL || 'localhost:7175';
const client = new grpc.Client();
client.load([], './protos/order.proto');

const successRate = new Rate('scenario_success_rate');
const responseTime = new Trend('scenario_response_time');
const requestCount = new Counter('scenario_requests_total');

export function runScenario() {
  requestCount.add(1);
  client.connect(webappUrl, { plaintext: false });

  const start = Date.now();
  const response = client.invoke('{proto.package.Service}/{Method}', {
    correlationId: crypto.randomUUID()
  });
  responseTime.add(Date.now() - start);

  const ok = check(response, { 'status is OK': r => r && r.status === grpc.StatusOK });
  successRate.add(ok);

  client.close();
  sleep(1);
}
```
