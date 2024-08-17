import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  thresholds: {
    http_req_duration: ['p(90) < 400', 'p(95) < 800', 'p(99.9) < 2000'],
    http_req_failed: ['rate<0.01'],
  },
};

export function getOrder() {
  const res = http.get('https://localhost:7175/orders/1');

  check(res, {
    'status is 200': (r) => r.status === 200,
    'content type is JSON': (r) => r.headers['Content-Type'] === 'application/json; charset=utf-8',
  });

  sleep(1);
}