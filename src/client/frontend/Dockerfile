FROM node:25 AS build
ARG VITE_APP_VERSION
WORKDIR /usr/src/app
COPY src/client/frontend/package.json src/client/frontend/package-lock.json ./
RUN npm i
COPY src/client/frontend/. ./
RUN npm run build

FROM nginx:1.29-alpine
COPY --from=build /usr/src/app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]