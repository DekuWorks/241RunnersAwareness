# Development Snippets

## Frontend: Post-Login Redirect

### authThunks.ts
```typescript
export const login = createAsyncThunk('auth/login', async (payload, { dispatch }) => {
  const { data } = await api.post('/api/v1/auth/login', payload);
  tokenStore.set(data.accessToken);
  await dispatch(fetchMe());
  return data;
});
```

### LoginPage.tsx
```typescript
const onSubmit = async (values) => {
  try {
    await dispatch(login(values)).unwrap();
    navigate('/profile');
  } catch (e) {
    toast.error(e.message ?? 'Login failed');
  }
};
```

## Axios Interceptor

```typescript
api.interceptors.request.use((config) => {
  const token = tokenStore.get();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (r) => r,
  (err) => {
    if (err?.response?.status === 401) {
      store.dispatch(logout());
      window.location.assign('/login');
    }
    return Promise.reject(err);
  }
);
```

## Backend: Ownership Guard

```csharp
if (runner.UserId != user.UserId && user.Role != Roles.Admin)
{
    throw new ForbiddenException();
}
```

## Manual QA Checklist

### Authentication Flow
- [ ] User can register with valid email/password
- [ ] User can login with valid credentials
- [ ] Invalid credentials show appropriate error
- [ ] Session persists across browser refresh
- [ ] 401 responses trigger automatic logout
- [ ] User is redirected to profile after login

### Profile Management
- [ ] Profile page loads with user data
- [ ] User can edit account information
- [ ] Changes persist after save
- [ ] Error messages are clear and helpful
- [ ] Loading states are shown during operations

### Runner Management
- [ ] User can create a new runner
- [ ] Runner appears in list immediately after creation
- [ ] User can edit own runners
- [ ] User cannot edit others' runners (403 error)
- [ ] Admin can view all runners
- [ ] Admin can edit/delete any runner
- [ ] Pagination works correctly
- [ ] Filters work as expected

### Error Handling
- [ ] Network errors show user-friendly messages
- [ ] 401 errors trigger logout
- [ ] 403 errors show access denied message
- [ ] 500 errors show generic error message
- [ ] Form validation errors are clear
- [ ] Loading states prevent double-submission
