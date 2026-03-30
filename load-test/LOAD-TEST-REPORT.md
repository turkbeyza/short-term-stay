# Load Testing Report

## 1. Endpoints Tested

| # | Endpoint | Method | Auth | Description |
|---|---|---|---|---|
| 1 | `/api/v1/Auth/login` | POST | Public | Authenticates a user and returns a JWT token |
| 2 | `/api/v1/Bookings` | POST | Guest (JWT) | Books a stay at a listing with date availability validation |

These endpoints were chosen because they represent the core user flow: authenticating and then making a booking - the two most critical operations under real-world load.

## 2. Test Scripts

The load testing was performed using **k6**. The test script (`load-tests/load-test.js`) performs the following in each iteration:

1. **Setup phase**: Registers a Host and Guest user, creates a test listing
2. **Test loop** (per VU): 
   - Sends a `POST /Auth/login` request
   - Sends a `POST /Bookings` request with unique dates per VU/iteration
   - Sleeps 1 second (simulating user think time)


## 3. Load Test Results



## 4. Analysis

The API performs well under **normal load (20 VUs)** with a 563ms average response time and 0% error rate, comfortably handling typical traffic. Under **peak load (50 VUs)**, response times nearly triple to 1.57s average and the p95 jumps to 3.96s, indicating that the server begins to saturate as concurrent connections increase. At **stress load (100 VUs)**, the average response time reaches 2.66s with p95 at 6.27s, though throughput still scales from 17.6 to 29.5 requests/sec showing the server queues requests rather than dropping them.

**Observed bottleneck:** The main bottleneck is the single-instance Azure App Service plan, which limit concurrent request handling capacity.

**Potential improvements:** Scaling to multiple App Service instances (horizontal scaling), adding response caching for read-heavy endpoints, and upgrading the Azure SQL tier would significantly improve performance under high concurrency.
