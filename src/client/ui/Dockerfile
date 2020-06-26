# The dotnet sdk image is used as a base image for the build as the nerdbank-gitversioning dev-dependency has a dependency on dotnet
FROM mcr.microsoft.com/dotnet/sdk:5.0.100-preview.6-buster-slim AS build
RUN curl -sL https://deb.nodesource.com/setup_14.x | bash -
RUN apt-get install -y nodejs
RUN npm install -g yarn
COPY ./.git /.git
WORKDIR /src/client/ui
COPY src/client/ui/package.json src/client/ui/yarn.lock ./
RUN yarn
COPY src/client/ui/. ./
RUN chmod +x node_modules/.bin/react-scripts
RUN yarn build

FROM nginx:1.19.0-alpine
COPY --from=build /src/client/ui/build /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]