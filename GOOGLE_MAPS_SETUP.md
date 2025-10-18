# Google Maps API Setup Guide

## Quick Setup Steps

1. **Get a Google Maps API Key:**
   - Go to [Google Cloud Console](https://console.cloud.google.com/)
   - Create a new project or select an existing one
   - Enable the following APIs:
     - Maps JavaScript API
     - Places API (optional, for search functionality)
     - Geocoding API (optional, for address lookup)

2. **Configure API Key:**
   - Open `google-maps-config.js` in your project
   - Replace `YOUR_GOOGLE_MAPS_API_KEY` with your actual API key
   - Save the file

3. **Set up API Key Restrictions (Recommended):**
   - In Google Cloud Console, go to "Credentials"
   - Click on your API key
   - Under "Application restrictions", choose "HTTP referrers"
   - Add your domain: `https://241runnersawareness.org/*`
   - Add localhost for testing: `http://localhost/*`

## Example Configuration

```javascript
// In google-maps-config.js
window.GOOGLE_MAPS_CONFIG = {
    API_KEY: "AIzaSyBxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", // Your actual key
    // ... rest of configuration
};
```

## Testing

1. Open `map.html` in your browser
2. Check the browser console for any errors
3. The map should load with Houston, TX as the center
4. You should see sample markers for demonstration

## Troubleshooting

- **404 Error**: Make sure `google-maps-config.js` exists in the root directory
- **API Key Error**: Verify your API key is correct and has the right permissions
- **Map Not Loading**: Check browser console for specific error messages
- **Billing**: Google Maps requires a billing account (but has free tier)

## Free Tier Limits

- Maps JavaScript API: 28,000 map loads per month
- Places API: 1,000 requests per month
- Geocoding API: 40,000 requests per month

For most small to medium websites, the free tier should be sufficient.
