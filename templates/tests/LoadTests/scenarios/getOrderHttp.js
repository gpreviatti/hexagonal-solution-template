import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  thresholds: {
    http_req_duration: ['p(50) < 250', 'p(95) < 150', 'p(99.9) < 100'],
    http_req_failed: ['rate<0.05'],
  },
};

export function getOrderHttp() {
  const headers = {
    headers: {
      'correlationId': crypto.randomUUID(),
      'Accept': 'application/json',
        'Accept-Encoding': 'gzip, deflate',
        'CacheEnabled': 'false'
    }
  };
  const webappUrl = __ENV.WEBAPP_URL || 'https://localhost:7175';

  const res = http.get(`${webappUrl}/orders/1`, headers);
  check(res, {
    'status is 200': (r) => r.status === 200,
    'content type is JSON': (r) => r.headers['Content-Type'] === 'application/json; charset=utf-8',
  });

  sleep(1);
}
