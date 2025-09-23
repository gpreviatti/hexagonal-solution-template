import { getOrder } from './scenarios/get-order.js';

export const options = {
  scenarios: {
    get_order: {
      exec: 'get_order',
      env: { EXAMPLEVAR: 'testing' },

      executor: 'constant-vus',
      vus: 10,
      duration: '30s',
      gracefulStop: '10s',
    },
  }
};

export function get_order() {
  getOrder();
}