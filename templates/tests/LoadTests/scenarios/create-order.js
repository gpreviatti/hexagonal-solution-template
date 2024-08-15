import http from 'k6/http';
import { check, sleep } from 'k6';
import { readFileSync } from 'k6/fs';

export const options = {
    vus: 10, // Number of virtual users
    duration: '30s', // Test duration
};

function createOrder() {
    const orderData = JSON.parse(readFileSync('./order_data.json'));
    const res = http.post('http://localhost:3000/orders', JSON.stringify(orderData), {
        headers: { 'Content-Type': 'application/json' },
    });
    check(res, {
        'status is 201': (r) => r.status === 201,
        'content type is JSON': (r) => r.headers['Content-Type'] === 'application/json; charset=utf-8',
    });
}

export default function () {
    createOrder();
    sleep(1); // Sleep for 1 second between requests
}