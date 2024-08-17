import { createOrder } from './scenarios/create-order.js';
import { getOrder } from './scenarios/get-order.js';

export const options = {
  scenarios: {
    create_order: {
      exec: 'create_order',
      env: { EXAMPLEVAR: 'testing' },

      executor: 'constant-vus',
      vus: 10,
      duration: '30s',
      gracefulStop: '10s',
    },
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

export function create_order() {
  createOrder();
}

export function get_order() {
  getOrder();
}