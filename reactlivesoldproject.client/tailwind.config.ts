// tailwind.config.js
/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./src/**/*.{js,jsx,ts,tsx}", // �Esta es la l�nea clave!
        "./public/index.html"
    ],
    theme: {
        extend: {},
    },
    plugins: [],
}