FROM node:22-alpine AS build

WORKDIR /app
COPY package.json bun.lock ./
RUN npm install

COPY . .
RUN npm run build

FROM node:22-alpine

WORKDIR /app
ENV NODE_ENV=production

COPY --from=build /app/package.json /app/bun.lock ./
RUN npm install
COPY --from=build /app/.output ./.output

EXPOSE 3001

CMD ["npm", "run", "preview", "--", "--host", "0.0.0.0"]
