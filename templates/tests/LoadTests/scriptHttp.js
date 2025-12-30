import http, { get } from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Trend, Rate } from 'k6/metrics';

export const options = {
  scenarios: {
    get_order: {
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
    get_order_response_time: ['p(95) < 500'],
    get_order_success_rate: ['rate>0.95'],
    get_order_requests_total: ['count>500']
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

const getOrderRequestsCounter = new Counter('get_order_requests_total');
const getOrderResponseTime = new Trend('get_order_response_time');
const getOrderSuccessRate = new Rate('get_order_success_rate');
export function getOrder() {
  getOrderRequestsCounter.add(1);
  
  const res = http.get(`${webappUrl}/orders/1`, headers);
  
  getOrderResponseTime.add(res.timings.duration);

  const checkResults = check(res, {
    'status is 200': (r) => r.status === 200,
    'content type is JSON': (r) => r.headers['Content-Type'] === 'application/json; charset=utf-8',
  });
  
  getOrderSuccessRate.add(checkResults);

  sleep(1);
}
