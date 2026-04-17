const target =
    process.env.API_URL ||
    process.env.ASPNETCORE_URLS?.split(';')[0] ||
    'https://localhost:5001';

module.exports = {
    "/api": {
        "target": target,
        "secure": false,
        "changeOrigin": true,
    }
}
