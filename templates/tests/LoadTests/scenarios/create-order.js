import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  thresholds: {
    http_req_duration: ['p(95)<250', 'p(99)<350'],
    http_req_failed: ['rate<0.01'],
  },
};

export function createOrder() {
  const orderData = JSON.stringify({
    Description: "John's computer",
    Items: [
      {
        Name: "Computer",
        Description: "Surface 2",
        Value: 1000
      },
      {
        Name: "Mouse",
        Description: "Microsoft mouse",
        "Value": 99
      },
      {
        Name: "Keyboard",
        Description: "Microsoft keyboard",
        Value: 199
      }
    ]
  });

  const res = http.post('https://localhost:7175/orders', orderData, {
    headers: { 'Content-Type': 'application/json' },
  });

  check(res, {
    'status is 201': (r) => r.status === 201,
    'content type is JSON': (r) => r.headers['Content-Type'] === 'application/json; charset=utf-8',
  });

  sleep(1);
}