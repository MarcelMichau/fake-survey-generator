FROM node:14.10.0-alpine3.12 as build
ARG REACT_APP_VERSION
WORKDIR /usr/src/app
COPY src/client/ui/package.json src/client/ui/yarn.lock ./
RUN yarn
COPY src/client/ui/. ./
RUN chmod +x node_modules/.bin/react-scripts
RUN yarn build

FROM nginx:1.19.2-alpine
COPY --from=build /usr/src/app/build /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]