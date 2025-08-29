# Setup and Run Instructions

## Prerequisites
- Node.js and npm
- .NET 9 SDK
- Azure Entra ID tenant with app registration

## Setup

### 1. Configure Frontend
```bash
cd frontend
cp src/environments/environment.sample.ts src/environments/environment.ts
```
Edit `environment.ts` and replace:
- `your-client-id-here` with your Azure app registration client ID
- `your-tenant-id-here` with your Azure tenant ID
- `your-api-client-id` with your backend API client ID

### 2. Configure Backend
```bash
cd backend
dotnet user-secrets set "AzureAd:TenantId" "your-tenant-id-here"
dotnet user-secrets set "AzureAd:ClientId" "your-api-client-id-here"
```

## Run Applications

### Backend
```bash
cd backend
dotnet run
```
Backend runs on: http://localhost:5219

### Frontend
```bash
cd frontend
npm install
npm start
```
Frontend runs on: http://localhost:4200

## Access
- Frontend: http://localhost:4200
- Backend Swagger: http://localhost:5219 (in development)