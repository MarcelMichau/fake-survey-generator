FROM oven/bun:alpine as build
ARG VITE_APP_VERSION
WORKDIR /usr/src/app
COPY src/client/ui/package.json src/client/ui/package-lock.json ./
RUN bun i
COPY src/client/ui/. ./
RUN bun run build

FROM nginx:1.27-alpine
COPY --from=build /usr/src/app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]