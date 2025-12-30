import { getOrderHttp } from './scenarios/getOrderHttp.js';

export const options = {
  scenarios: {
    getOrderHttp: {
      exec: 'get_order_http',
      executor: 'constant-vus',
      vus: 10,
      duration: '60s',
      gracefulStop: '10s'
    }
  }
};

export function get_order_http() {
  getOrderHttp();
}
