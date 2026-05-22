# Load Test Commands

```bash
k6 run tests/LoadTests/scriptHttp.js --summary-mode=full

k6 run tests/LoadTests/scriptGrpc.js --summary-mode=full

k6 run -e VUS=25 -e DURATION=120s -e WEBAPP_URL=https://localhost:7175 tests/LoadTests/scriptHttp.js

k6 run -e VUS=25 -e DURATION=120s -e WEBAPP_GRPC_URL=localhost:7175 tests/LoadTests/scriptGrpc.js
```
