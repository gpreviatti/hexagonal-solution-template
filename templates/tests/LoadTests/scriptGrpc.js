import grpc from 'k6/net/grpc';
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
    grpc_req_duration: ['p(50) < 50', 'p(95) < 100', 'p(99.9) < 500'],
    get_order_response_time: ['p(95) < 100'],
    get_order_success_rate: ['rate>0.95'],
    get_order_requests_total: ['count>500']
  }
};

const webappUrl = __ENV.WEBAPP_GRPC_URL || 'localhost:7175';

const client = new grpc.Client();
client.load([], './protos/order.proto');

const getOrderRequestsCounter = new Counter('get_order_requests_total');
const getOrderResponseTime = new Trend('get_order_response_time');
const getOrderSuccessRate = new Rate('get_order_success_rate');
export function getOrder() {
  getOrderRequestsCounter.add(1);
  
  client.connect(webappUrl, { plaintext: false });

  const startTime = Date.now();
  const request = { id: 1, correlationId: crypto.randomUUID() };
  const response = client.invoke('order.OrderService/Get', request);
  const duration = Date.now() - startTime;
  
  getOrderResponseTime.add(duration);

  const checkResults = check(response, {
    'status is OK': (r) => r && r.status === grpc.StatusOK,
    'success': (r) => r.message && r.message.success === true,  
  });
  
  getOrderSuccessRate.add(checkResults);

  client.close();
  sleep(1);
}
