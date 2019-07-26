FROM node:12.7.0-alpine as build
WORKDIR /usr/src/app
COPY FakeSurveyGenerator.UI/ui/package.json yarn.lock ./
RUN yarn
COPY FakeSurveyGenerator.UI/ui/. ./
RUN yarn build

FROM nginx:1.17.2-alpine
COPY --from=build /usr/src/app/build /etc/nginx/html
# COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]