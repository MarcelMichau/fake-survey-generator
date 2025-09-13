FROM oven/bun AS build
ARG VITE_APP_VERSION
WORKDIR /usr/src/app
COPY src/client/ui/package.json src/client/ui/bun.lockb ./
RUN bun i
COPY src/client/ui/. ./
RUN bun run build

FROM nginx:1.29.1-alpine
COPY --from=build /usr/src/app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]