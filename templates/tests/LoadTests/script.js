import { getOrderHttp } from './scenarios/get-order-http.js';
import { getOrderGrpc } from './scenarios/get-order-grpc.js';

export const options = {
  scenarios: {
    get_order_http: {
      exec: 'get_order_http',
      env: { EXAMPLEVAR: 'testing' },

      executor: 'constant-vus',
      vus: 10,
      duration: '30s',
      gracefulStop: '10s',
    },
    get_order_grpc: {
      exec: 'get_order_grpc',
      env: { EXAMPLEVAR: 'testing' },

      executor: 'constant-vus',
      vus: 10,
      duration: '30s',
      gracefulStop: '10s',
    }
  }
};

export function get_order_http() {
  getOrderHttp();
}

export function get_order_grpc() {
  getOrderGrpc();
}