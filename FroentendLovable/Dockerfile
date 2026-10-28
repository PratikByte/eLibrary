FROM node:22-alpine AS build

WORKDIR /app
COPY package.json bun.lock ./
RUN npm install

COPY . .
ARG VITE_API_BASE_URL=http://localhost:8000
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

FROM node:22-alpine

WORKDIR /app
ENV NODE_ENV=production
ENV NITRO_HOST=0.0.0.0
ENV NITRO_PORT=3001

COPY --from=build /app/package.json /app/bun.lock ./
RUN npm install
COPY --from=build /app/.output ./.output

EXPOSE 3001

CMD ["node", ".output/server/index.mjs"]
