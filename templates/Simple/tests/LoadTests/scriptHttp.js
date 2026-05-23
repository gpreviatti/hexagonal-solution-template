import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Trend, Rate } from 'k6/metrics';

export const options = {
  scenarios: {
    get_order: {
      exec: 'getOrder',
      executor: 'constant-vus',
      vus: __ENV.VUS ? parseInt(__ENV.VUS) : 10,
      duration: __ENV.DURATION ? __ENV.DURATION : '60s',
      gracefulStop: __ENV.GRACEFUL_STOP ? __ENV.GRACEFUL_STOP : '10s'
    },
    create_order: {
        exec: 'createOrder',
        executor: 'constant-vus',
        vus: __ENV.VUS ? parseInt(__ENV.VUS) : 10,
        duration: __ENV.DURATION ? __ENV.DURATION : '60s',
        gracefulStop: __ENV.GRACEFUL_STOP ? __ENV.GRACEFUL_STOP : '10s'
    }
  },
  thresholds: {
        http_req_duration: ['p(50) < 100', 'p(95) < 500', 'p(99.9) < 1000'],
        http_req_failed: ['rate<0.1'],
        get_order_response_time: ['p(95) < 500'],
        get_order_success_rate: ['rate>0.95'],
        get_order_requests_total: ['count>500'],
        create_order_response_time: ['p(95) < 500'],
        create_order_success_rate: ['rate>0.95'],
        create_order_requests_total: ['count>500']
    },
};

const webappUrl = __ENV.WEBAPP_URL || 'https://localhost:7175';

const headers = {
  headers: {
    'correlationId': crypto.randomUUID(),
    'Content-Type': 'application/json',
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

const createOrderRequestsCounter = new Counter('create_order_requests_total');
const createOrderResponseTime = new Trend('create_order_response_time');
const createOrderSuccessRate = new Rate('create_order_success_rate');
export function createOrder() {
    createOrderRequestsCounter.add(1);

    const payload = JSON.stringify({
        CorrelationId: crypto.randomUUID(),
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
                Value: 99
            }
        ]
    });

    const res = http.post(`${webappUrl}/orders`, payload, headers);

    createOrderResponseTime.add(res.timings.duration);

    const checkResults = check(res, {
        'status is 201': (r) => r.status === 201,
        'content type is JSON': (r) => r.headers['Content-Type'] === 'application/json; charset=utf-8',
    });

    createOrderSuccessRate.add(checkResults);

    sleep(1);
}
