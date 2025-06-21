# 241Runners Awareness Application

This is the official web application for the 241 Runners Awareness organization. It provides a secure portal for individuals to manage their case files and a comprehensive dashboard for administrators to manage user data, cases, and system logs.

## ✨ Features

- **Authentication:** Secure user login and session management using JWT.
- **Role-Based Access Control:** Distinct interfaces and permissions for regular users and administrators.
- **Admin Dashboard:** A central hub for admins to manage users, view all case files, and monitor system activity via an audit log.
- **Case Management:** Users can view their personal case details and update their profile photo.
- **Secure Photo Uploads:** Users can upload a new photo, which is restricted to once every 30 days to prevent abuse.
- **Profile Reminders:** Automatic reminders notify users when their profile photo needs to be updated (every 6 months).

## 🚀 Tech Stack

- **Frontend:**
  - [React](https://reactjs.org/)
  - [Vite](https://vitejs.dev/)
  - [Redux Toolkit](https://redux-toolkit.js.org/) for state management
  - [Axios](https://axios-http.com/) for API requests
  - [Tailwind CSS](https://tailwindcss.com/) for styling
- **Backend:**
  - C# with ASP.NET Core
  - Entity Framework Core for data access
- **Deployment:**
  - Configured for easy deployment to [Netlify](https://www.netlify.com/).

## 🛠️ Local Development Setup

Follow these steps to get the frontend running on your local machine.

### 1. Prerequisites
- [Node.js](https://nodejs.org/en/) (v18 or higher recommended)
- A running instance of the 241RunnersAwareness backend API.

### 2. Clone the Repository
```bash
git clone <your-repository-url>
cd 241runnersawareness-frontend
```

### 3. Install Dependencies
```bash
npm install
```

### 4. Configure Environment Variables
Copy the example environment file to create your own local configuration:

```bash
cp env.example .env
```
Now, open the `.env` file and fill in the required values for your local backend API.
```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_AUTH_TOKEN_KEY=authToken
VITE_APP_NAME=241Runners Awareness
```

### 5. Run the Development Server
```bash
npm run dev
```
The application should now be running at `http://localhost:5173` (or the next available port).
