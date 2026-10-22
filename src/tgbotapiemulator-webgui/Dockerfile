FROM node:26-alpine AS build
WORKDIR /fe
COPY package*.json ./
RUN npm ci
COPY . ./
RUN npm run build

FROM node:26-alpine
WORKDIR /fe
ENV NODE_ENV=production
ENV PORT=3000
COPY --from=build /fe/.next ./.next
COPY --from=build /fe/public ./public
COPY --from=build /fe/package*.json ./
COPY --from=build /fe/next.config.* ./
RUN npm ci --omit=dev
EXPOSE 3000
CMD ["npx", "next", "start", "-p", "3000"]
