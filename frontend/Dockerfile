# Stage 1: Dependencies
FROM node:22-alpine AS deps
RUN apk add --no-cache libc6-compat
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci

# Stage 2: Builder
FROM node:22-alpine AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
RUN mkdir -p /app/public
# NEXT_PUBLIC_* được nhúng lúc build nên phải truyền qua build arg (xem docker-compose.yml)
# rewrites() trong next.config.ts được tính lúc BUILD (next start chỉ đọc lại từ manifest), nên địa chỉ backend
# phải có ngay lúc build; đặt biến môi trường lúc chạy là không đủ.
ARG NEXT_PUBLIC_SITE_URL=http://localhost:3000
ARG NEXT_PUBLIC_GOOGLE_CLIENT_ID=
ARG NEXT_PUBLIC_AVATAR_UPLOAD_ENABLED=true
ARG BACKEND_API_URL=http://api:8080
ENV BACKEND_API_URL=$BACKEND_API_URL
ENV NEXT_PUBLIC_SITE_URL=$NEXT_PUBLIC_SITE_URL
ENV NEXT_PUBLIC_GOOGLE_CLIENT_ID=$NEXT_PUBLIC_GOOGLE_CLIENT_ID
ENV NEXT_PUBLIC_AVATAR_UPLOAD_ENABLED=$NEXT_PUBLIC_AVATAR_UPLOAD_ENABLED
ENV NEXT_TELEMETRY_DISABLED=1
RUN npm run build

# Stage 3: Runner
FROM node:22-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

COPY --from=builder /app/public ./public
COPY --from=builder --chown=nextjs:nodejs /app/.next ./.next
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/package.json ./package.json
# next start nạp lại next.config lúc chạy; thiếu tệp này thì images.remotePatterns mất và mọi ảnh ngoài bị 400
COPY --from=builder /app/next.config.ts ./next.config.ts
ARG BACKEND_API_URL=http://api:8080
ENV BACKEND_API_URL=$BACKEND_API_URL

USER nextjs
EXPOSE 3000

CMD ["npm", "start"]
