# 241 Runners Awareness

This is the official application for the 241 Runners Awareness organization. It includes a public-facing informational website, a portal for registered individuals to manage their case files, and an administrative dashboard for staff.

## Tech Stack

- **Frontend:** React, Redux, Vite, Tailwind CSS
- **Backend:** C# (.NET), ASP.NET Core, Entity Framework Core
- **Database:** SQL Server

## Setup Steps

1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-repo/241RunnersAwareness.git
   ```
2. **Backend Setup:**
   - Navigate to the `241RunnersAwareness.BackendAPI` directory.
   - Restore dependencies: `dotnet restore`
   - Update database: `dotnet ef database update`
   - Run the backend: `dotnet run`
3. **Frontend Setup:**
   - Navigate to the `241runnersawareness-frontend` directory.
   - Install dependencies: `npm install`
   - Start the development server: `npm run dev`

## Environment Variables
Create a `.env` file in the `241runnersawareness-frontend` directory and add the following variables. See `.env.example` for a template.

# React + Vite

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Babel](https://babeljs.io/) for Fast Refresh
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/) for Fast Refresh

## Expanding the ESLint configuration

If you are developing a production application, we recommend using TypeScript with type-aware lint rules enabled. Check out the [TS template](https://github.com/vitejs/vite/tree/main/packages/create-vite/template-react-ts) for information on how to integrate TypeScript and [`typescript-eslint`](https://typescript-eslint.io) in your project.
