import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  scenarios: {
    getOrderHttp: {
      exec: 'getOrder',
      executor: 'constant-vus',
      vus: 10,
      duration: '60s',
      gracefulStop: '10s'
    }
  },
  thresholds: {
    http_req_duration: ['p(50) < 100', 'p(95) < 500', 'p(99.9) < 1000'],
    http_req_failed: ['rate<0.1'],
  },
};

const webappUrl = __ENV.WEBAPP_URL || 'https://localhost:7175';

const headers = {
  headers: {
    'correlationId': crypto.randomUUID(),
    'Accept': 'application/json',
    'Accept-Encoding': 'gzip, deflate',
    'CacheEnabled': 'false'
  }
};
  
export function getOrder() {
  const res = http.get(`${webappUrl}/orders/1`, headers);
  check(res, {
    'status is 200': (r) => r.status === 200,
    'content type is JSON': (r) => r.headers['Content-Type'] === 'application/json; charset=utf-8',
  });

  sleep(1);
}
