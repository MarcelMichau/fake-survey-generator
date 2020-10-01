module.exports = {
    future: {
        removeDeprecatedGapUtilities: true,
        purgeLayersByDefault: true,
    },
    purge: ["./public/**/*.html", "./src/**/*.tsx"],
    theme: {
        extend: {},
    },
    variants: {},
    plugins: [],
    dark: "media",
    experimental: {
        darkModeVariant: true,
    },
};
