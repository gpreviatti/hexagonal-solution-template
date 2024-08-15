import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    vus: 10, // Number of virtual users
    duration: '30s', // Test duration
};

function getOrder() {
    const res = http.get('http://localhost:3000/orders/123');
    check(res, {
        'status is 200': (r) => r.status === 200,
        'content type is JSON': (r) => r.headers['Content-Type'] === 'application/json; charset=utf-8',
    });
}

export default function () {
    getOrder();
    sleep(1); // Sleep for 1 second between requests
}