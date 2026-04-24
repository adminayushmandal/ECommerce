// const target =
//     process.env.API_URL ||
//     process.env.ASPNETCORE_URLS?.split(';')[0] ||
//     'https://localhost:5001';

const target =
    process.env["services__api__https__0"] ||
    process.env["services__api__http__0"]

console.log("Proxy target:", target);

module.exports = {
    "/api": {
        target,
        secure: false,
        changeOrigin: true,
        logLevel: "debug",
    },
    "/hubs": {
        target,
        secure: false,
        changeOrigin: true,
        ws: true,
        logLevel: "debug",
    }
};
