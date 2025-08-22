/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html","./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      colors:{
        bg:"var(--bg)", panel:"var(--panel)", text:"var(--text)", muted:"var(--muted)",
        accent:"var(--accent)", warn:"var(--warn)", ok:"var(--ok)"
      },
      borderRadius:{ DEFAULT:"var(--radius)", sm:"var(--radius-sm)", lg:"var(--radius-lg)" },
      boxShadow:{ soft:"var(--shadow)" }
    },
  },
  plugins: [],
} 