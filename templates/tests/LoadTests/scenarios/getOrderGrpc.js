import grpc from 'k6/net/grpc';
import { check, sleep } from 'k6';

export const options = {
    thresholds: {
        http_req_duration: ['p(50) < 1000', 'p(95) < 400', 'p(99.9) < 100'],
        http_req_failed: ['rate<0.01'],
    },
};


const client = new grpc.Client();
client.load([], './protos/order.proto'); // Load your .proto file

export function getOrderGrpc() {
    let webappUrl = __ENV.WEBAPP_URL || 'localhost:7175';

    // Remove protocol scheme (http:// or https://) for gRPC connection
    webappUrl = webappUrl.replace(/^http?:\/\//, '');

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
