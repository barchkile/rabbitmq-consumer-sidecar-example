FROM node:16.13-alpine

COPY my-app/package.json ./package.json
RUN npm i

COPY my-app .

EXPOSE 3000

ENTRYPOINT [ "npm", "start" ]