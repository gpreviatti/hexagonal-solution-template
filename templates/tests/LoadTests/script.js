import { getOrderHttp } from './scenarios/getOrderHttp.js';
import { getOrderGrpc } from './scenarios/getOrderGrpc.js';

export const options = {
  scenarios: {
    getOrderHttp: {
      exec: 'get_order_http',
      executor: 'constant-vus',
      vus: 10,
      duration: '60s',
      gracefulStop: '10s'
    },
    getOrderGrpc: {
      exec: 'get_order_grpc',
      executor: 'constant-vus',
      vus: 10,
      duration: '60s',
      gracefulStop: '10s',
      startTime: '70s',
    }
  }
};

export function get_order_http() {
  getOrderHttp();
}

export function get_order_grpc() {
  getOrderGrpc();
}
