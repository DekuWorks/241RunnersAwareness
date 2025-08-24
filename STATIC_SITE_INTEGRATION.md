# Static Site & React App Integration

## Overview

The 241 Runners Awareness platform now supports both static HTML pages and a React application, both connecting to the same backend API. This ensures users can access the same functionality regardless of which platform they use.

## Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Static Site   │    │   React App     │    │   Backend API   │
│   (HTML/CSS/JS) │    │   (React/Redux) │    │   (.NET Core)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    ┌─────────────────┐
                    │   Shared API    │
                    │   Endpoints     │
                    └─────────────────┘
```

## Shared Features

### ✅ **Runner Profile System**
Both platforms now support the complete runner profile functionality:

#### **Static Site (`runner-profile.html`)**
- **URL**: `runner-profile.html?id=1`
- **Features**:
  - Sticky header with runner photo, name, and status
  - Compact action bar with keyboard shortcuts (E/U/C)
  - Tabbed interface (Timeline, Cases, Photos, Details)
  - Role-based access control (owner/admin only)
  - Real API integration with fallback to mock data
  - Responsive design with Tailwind CSS
  - Accessibility features (ARIA roles, focus management)

#### **React App (`RunnerProfilePage.jsx`)**
- **URL**: `/runners/:id`
- **Features**:
  - Same UI/UX as static site
  - Redux state management
  - Component-based architecture
  - TypeScript support (ready)
  - Advanced state management

### ✅ **Navigation Integration**
Both platforms share consistent navigation:

#### **Static Site Navigation**
- Added "🏃 Runner Profile" link to all authenticated pages
- Updated `auth-utils.js` to handle runner profile navigation
- Consistent with existing dashboard, report case, and my cases links

#### **React App Navigation**
- Added `/runners/:id` route with authentication protection
- Integrated with existing React Router setup
- Consistent with other protected routes

## API Integration

### **Backend Endpoints**
Both platforms use the same API endpoints:

```javascript
// Runner Profile API
GET /api/individuals/{id}           // Get runner profile
POST /api/individuals               // Create runner
PUT /api/individuals/{id}           // Update runner
DELETE /api/individuals/{id}        // Delete runner

// Photos API
GET /api/individuals/{id}/photos    // Get runner photos
POST /api/individuals/{id}/photos   // Upload photo
PUT /api/individuals/{id}/photos/{photoId}  // Update photo
DELETE /api/individuals/{id}/photos/{photoId} // Delete photo

// Activities API
GET /api/individuals/{id}/activities // Get timeline activities
```

### **Authentication**
Both platforms use the same authentication system:

```javascript
// JWT Token Authentication
const token = localStorage.getItem('userToken');
const headers = {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
};
```

### **Error Handling**
Both platforms implement graceful fallbacks:

```javascript
// Static Site (runner-profile.html)
try {
    const response = await fetch(`${API_BASE}/individuals/${runnerId}`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });
    if (response.ok) {
        runnerData = await response.json();
    } else {
        throw new Error('API call failed');
    }
} catch (apiError) {
    // Fallback to mock data
    runnerData = { /* mock data */ };
}

// React App (casesSlice.js)
export const getIndividual = createAsyncThunk(
    'cases/getIndividual',
    async (id, { rejectWithValue }) => {
        try {
            const response = await api.get(`/individuals/${id}`);
            return response.data;
        } catch (error) {
            return rejectWithValue(error.response?.data || 'Failed to fetch individual');
        }
    }
);
```

## Database Consistency

### **Shared Models**
Both platforms work with the same database models:

```csharp
// Backend Models (Individual.cs)
public class Individual
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string RunnerId { get; set; }
    public string Status { get; set; }
    public Guid? OwnerUserId { get; set; }
    public virtual ICollection<Photo> Photos { get; set; }
    public virtual ICollection<Activity> Activities { get; set; }
    public virtual ICollection<Case> Cases { get; set; }
}
```

### **Data Flow**
1. **User signs up** on either static site or React app
2. **User creates runner profile** using either platform
3. **Data is stored** in the same SQLite/SQL Server database
4. **Both platforms can access** the same runner data
5. **Real-time updates** are reflected across both platforms

## User Experience

### **Seamless Transition**
Users can switch between platforms without losing data:

1. **Sign up** on static site (`signup.html`)
2. **Create runner profile** on React app (`/runners/new`)
3. **View profile** on static site (`runner-profile.html?id=1`)
4. **Edit profile** on React app (`/runners/1/edit`)
5. **All changes persist** across both platforms

### **Feature Parity**
Both platforms offer the same core features:

| Feature | Static Site | React App | Status |
|---------|-------------|-----------|---------|
| Runner Profile View | ✅ | ✅ | Complete |
| Photo Upload | 🔄 | 🔄 | In Progress |
| Status Updates | 🔄 | 🔄 | In Progress |
| Timeline Activity | 🔄 | 🔄 | In Progress |
| Case Management | ✅ | ✅ | Complete |
| Role-based Access | ✅ | ✅ | Complete |
| Keyboard Shortcuts | ✅ | ✅ | Complete |
| Responsive Design | ✅ | ✅ | Complete |

## Development Workflow

### **Adding New Features**
1. **Backend First**: Implement API endpoints in `.NET Core`
2. **Static Site**: Add functionality to HTML pages
3. **React App**: Add components and Redux slices
4. **Test Both**: Ensure feature works on both platforms
5. **Deploy**: Update both static site and React app

### **Code Sharing**
- **API Integration**: Shared authentication and error handling patterns
- **Styling**: Both use Tailwind CSS for consistent design
- **Validation**: Shared backend validation rules
- **Security**: Same JWT authentication and RBAC

## Deployment

### **Static Site**
- **Hosting**: Netlify/Vercel/GitHub Pages
- **Files**: HTML, CSS, JS files in root directory
- **API**: Points to same backend as React app

### **React App**
- **Hosting**: Netlify/Vercel/VPS
- **Build**: `npm run build` creates optimized bundle
- **API**: Points to same backend as static site

### **Backend API**
- **Hosting**: Azure/AWS/VPS
- **Database**: SQLite (dev) / SQL Server (prod)
- **CORS**: Configured for both static site and React app domains

## Benefits

### **For Users**
- **Choice**: Use preferred platform (static or React)
- **Consistency**: Same features and data across platforms
- **Reliability**: Fallback options if one platform is unavailable
- **Performance**: Static site loads faster, React app more interactive

### **For Developers**
- **Flexibility**: Choose best tool for each feature
- **Maintainability**: Shared backend logic
- **Testing**: Can test features on both platforms
- **Deployment**: Independent deployment cycles

### **For Organization**
- **Accessibility**: Static site works without JavaScript
- **SEO**: Static pages better for search engines
- **Progressive Enhancement**: Start simple, add complexity
- **Cost**: Static hosting is cheaper than React hosting

## Next Steps

### **Immediate Priorities**
1. **Photo Upload**: Implement file upload on both platforms
2. **Status Updates**: Add inline status change functionality
3. **Timeline**: Implement activity feed for both platforms
4. **Search**: Add fuzzy search and filtering

### **Future Enhancements**
1. **Real-time Updates**: WebSocket integration for live updates
2. **Offline Support**: Service Worker for static site
3. **Mobile Apps**: React Native using same API
4. **Advanced Features**: Map integration, notifications, etc.

## Conclusion

The integration of static site and React app provides a robust, flexible platform that serves users regardless of their technical preferences or device capabilities. Both platforms share the same backend API, ensuring data consistency and feature parity while allowing for platform-specific optimizations.
