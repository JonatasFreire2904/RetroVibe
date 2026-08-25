import type { Config } from "tailwindcss";

export default {
  content: ["./index.html", "./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      colors: {
        brand: {
          violet: "#7C3AED",
          fuchsia: "#EC4899",
          cyan: "#06B6D4",
        },
      },
      backgroundImage: {
        "brand-gradient": "linear-gradient(90deg, #7C3AED 0%, #EC4899 100%)",
      },
    },
  },
  plugins: [],
} satisfies Config;
