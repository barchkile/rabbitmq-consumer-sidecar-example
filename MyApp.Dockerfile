FROM node:16.13-alpine

COPY ./package.json ./package.json
RUN npm i

COPY . .

EXPOSE 3000

ENTRYPOINT [ "npm", "start" ]