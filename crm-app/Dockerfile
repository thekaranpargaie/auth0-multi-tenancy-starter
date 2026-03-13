# Stage 1: Build the Vue app
FROM node:18-alpine AS build
WORKDIR /app

# Declare build-time variables so Vite can embed them at build time
ARG VITE_AUTH0_DOMAIN
ARG VITE_CRM_CLIENT_ID
ARG VITE_AUTH0_AUDIENCE
ARG VITE_CRM_REDIRECT_URI
ARG VITE_API_BASE_URL

ENV VITE_AUTH0_DOMAIN=$VITE_AUTH0_DOMAIN
ENV VITE_CRM_CLIENT_ID=$VITE_CRM_CLIENT_ID
ENV VITE_AUTH0_AUDIENCE=$VITE_AUTH0_AUDIENCE
ENV VITE_CRM_REDIRECT_URI=$VITE_CRM_REDIRECT_URI
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

# Copy package files and install dependencies
COPY package*.json ./
RUN npm install

# Copy source code
COPY . .

# Build the app
RUN npm run build

# Stage 2: Serve the app with nginx
FROM nginx:alpine

# Copy built Vue app
COPY --from=build /app/dist /usr/share/nginx/html

# Copy nginx configuration for SPA routing
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost/health || exit 1

# Start nginx
CMD ["nginx", "-g", "daemon off;"]