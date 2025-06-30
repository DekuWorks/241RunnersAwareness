# 241RunnersAwareness.org

**Secure full-stack platform for reporting and managing missing individuals.**  
Built with React, .NET 8, and modern deployment tools, this system empowers users, caregivers, therapists, and law enforcement with secure tools to support recovery efforts.

🌐 [Visit Live Site](https://www.241runnersawareness.org)

---

## 🚀 Features

### 🔐 Authentication
- Google SSO Login
- Email/Password Signup + 2FA
- Role-based access control (User, Parent, Caregiver, ABA Therapist, Admin)
- Email verification via SendGrid
- SMS verification via Twilio

### 👥 User & Runner Management
- Custom registration forms based on user roles
- Emergency contact management
- Runner profiles with photo and biometric support
- Track adoption status and special needs context
- Photo upload/update features for case records

### 🧑‍💻 Admin Dashboard
- Full CRUD for users and case records
- Case tagging with status indicators: Missing, Found, Urgent, Resolved
- Real-time search, filter, and pagination
- Role-based permissions for sensitive actions
- Audit logging

### 🧰 Tech Stack

**Frontend:**
- React (Vite)
- Redux Toolkit
- TailwindCSS
- React Hook Form
- Radix UI
- React Router

**Backend:**
- ASP.NET Core 8 Web API
- Entity Framework Core
- SQL (with migrations)
- JWT Authentication
- Google OAuth

**DevOps:**
- CI/CD via Netlify
- Backend on Render
- Monorepo structure
- Swagger API documentation
- .env and CORS secured

---

## 📁 Project Structure

