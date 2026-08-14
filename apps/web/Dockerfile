FROM node:22-alpine AS dependencies

WORKDIR /app

COPY apps/web/package.json ./
COPY apps/web/package-lock.json ./

RUN npm ci

FROM node:22-alpine AS build

WORKDIR /app

COPY --from=dependencies \
    /app/node_modules \
    ./node_modules

COPY apps/web/ .

ARG NEXT_PUBLIC_API_URL=http://localhost:8080

ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL

RUN npm run build

FROM node:22-alpine AS runtime

WORKDIR /app

ENV NODE_ENV=production

COPY --from=build \
    /app/package.json \
    ./package.json

COPY --from=build \
    /app/package-lock.json \
    ./package-lock.json

COPY --from=build \
    /app/node_modules \
    ./node_modules

COPY --from=build \
    /app/.next \
    ./.next

COPY --from=build \
    /app/public \
    ./public

EXPOSE 3000

CMD ["npm", "start"]