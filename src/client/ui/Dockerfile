FROM node:18-alpine3.14 as build
ARG REACT_APP_VERSION
WORKDIR /usr/src/app
COPY src/client/ui/package.json src/client/ui/package-lock.json ./
RUN npm i
COPY src/client/ui/. ./
RUN chmod +x node_modules/.bin/react-scripts
RUN npm run build

FROM nginx:1.22.0-alpine
COPY --from=build /usr/src/app/build /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]