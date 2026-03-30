import http from "k6/http";
import { check, sleep } from "k6";
import { textSummary } from "https://jslib.k6.io/k6-summary/0.0.1/index.js";

http.setResponseCallback(http.expectedStatuses({ min: 200, max: 499 }));

// ─── Configuration ───────────────────────────────────────────────
const BASE_URL =
    "https://wa-short-term-gateway-faegbyf9exh7d7bz.switzerlandnorth-01.azurewebsites.net";

// ─── Thresholds ──────────────────────────────────────────────────
export const options = {
    thresholds: {
        http_req_duration: ["p(95)<2000"],
        http_req_failed: ["rate<0.05"],
    },
};

export function handleSummary(data) {
    return {
        stdout: textSummary(data, { indent: " ", enableColors: true }),
    };
}

// ─── Auth helper: register + login to get a JWT token ────────────
const timestamp = Date.now();

function getToken(role, index) {
    const username = `k6_${role}_${timestamp}_${index}`;
    const password = "TestPassword123!";

    http.post(
        `${BASE_URL}/api/v1/Auth/register`,
        JSON.stringify({
            username: username,
            password: password,
            role: role,
        }),
        { headers: { "Content-Type": "application/json" } }
    );

    const loginRes = http.post(
        `${BASE_URL}/api/v1/Auth/login`,
        JSON.stringify({
            username: username,
            password: password,
        }),
        { headers: { "Content-Type": "application/json" } }
    );

    if (loginRes.status === 200) {
        const body = JSON.parse(loginRes.body);
        return body.token;
    }
    return null;
}

export function setup() {
    const hostToken = getToken("Host", 0);
    const guestToken = getToken("Guest", 0);

    let listingId = null;
    if (hostToken) {
        const res = http.post(
            `${BASE_URL}/api/v1/Listings`,
            JSON.stringify({
                noOfPeople: 4,
                country: "Turkey",
                city: "Istanbul",
                price: 120.0,
            }),
            {
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${hostToken}`,
                },
            }
        );
        if (res.status === 200) {
            const body = JSON.parse(res.body);
            listingId = body.data.id;
        }
    }

    return { hostToken, guestToken, listingId };
}

export default function (data) {
    const loginRes = http.post(
        `${BASE_URL}/api/v1/Auth/login`,
        JSON.stringify({
            username: `k6_Guest_${timestamp}_0`,
            password: "TestPassword123!",
        }),
        { headers: { "Content-Type": "application/json" } }
    );

    check(loginRes, {
        "POST /Auth/login status is 200": (r) => r.status === 200,
        "POST /Auth/login response time < 2s": (r) => r.timings.duration < 2000,
    });

    if (data.guestToken && data.listingId) {
        const iter = __ITER;
        const vu = __VU;
        const dayOffset = vu * 1000 + iter * 2;
        const fromDate = new Date(2030, 0, 1 + dayOffset).toISOString();
        const toDate = new Date(2030, 0, 2 + dayOffset).toISOString();

        const bookingRes = http.post(
            `${BASE_URL}/api/v1/Bookings`,
            JSON.stringify({
                listingId: data.listingId,
                from: fromDate,
                to: toDate,
                namesOfPeople: `VU${vu}_Iter${iter}`,
            }),
            {
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${data.guestToken}`,
                },
            }
        );

        check(bookingRes, {
            // 200 = booked, 400 = date conflict (expected under load)
            "POST /Bookings server responded": (r) => r.status === 200 || r.status === 400,
            "POST /Bookings response time < 2s": (r) => r.timings.duration < 2000,
        });
    }

    sleep(1);
}
