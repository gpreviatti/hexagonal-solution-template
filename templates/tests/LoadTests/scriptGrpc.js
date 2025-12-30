import grpc from 'k6/net/grpc';
import { check, sleep } from 'k6';

export const options = {
  scenarios: {
    getOrderGrpc: {
      exec: 'get_order_grpc',
      executor: 'constant-vus',
      vus: 10,
      duration: '60s',
      gracefulStop: '10s'
    }
  },
  thresholds: {
    grpc_req_duration: ['p(50) < 50', 'p(95) < 100', 'p(99.9) < 500']
  }
};

const webappUrl = __ENV.WEBAPP_GRPC_URL || 'localhost:7175';

const client = new grpc.Client();
client.load([], './protos/order.proto');

export function get_order_grpc() {
  client.connect(webappUrl, { plaintext: false });

  const request = { id: 1, correlationId: crypto.randomUUID() };
  const response = client.invoke('order.OrderService/Get', request);

  check(response, {
    'status is OK': (r) => r && r.status === grpc.StatusOK,
    'success': (r) => r.message && r.message.success === true,  
  });

  client.close();
  sleep(1);
}
