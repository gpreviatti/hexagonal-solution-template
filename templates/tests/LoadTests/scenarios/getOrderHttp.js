import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  thresholds: {
    http_req_duration: ['p(50) < 2000', 'p(95) < 800', 'p(99.9) < 200'],
    http_req_failed: ['rate<0.01'],
  },
};

export function getOrderHttp() {
  const headers = {
    headers: {
      'correlationId': crypto.randomUUID(),
      'Accept': 'application/json',
      'Accept-Encoding': 'gzip, deflate',
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
