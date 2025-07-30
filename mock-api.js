// Enhanced Mock API for Houston Map Data
// This provides comprehensive sample data for the Houston area map

// Houston coordinates
const HOUSTON_LAT = 29.7604;
const HOUSTON_LNG = -95.3698;

// Mock API endpoints for testing
const mockApi = {
  '/api/auth/login': {
    method: 'POST',
    response: (data) => {
      console.log('Mock login attempt:', data);
      if (data.email && data.password) {
        return {
          success: true,
          message: 'Login successful (mock)',
          token: 'mock-jwt-token',
          user: {
            id: 'mock-user-id',
            email: data.email,
            fullName: 'Mock User',
            role: 'user'
          }
        };
      } else {
        return {
          success: false,
          message: 'Invalid credentials (mock)'
        };
      }
    }
  },
  '/api/auth/google-login': {
    method: 'POST',
    response: (data) => {
      console.log('Mock Google login attempt:', data);
      return {
        success: true,
        message: 'Google login successful (mock)',
        token: 'mock-jwt-token',
        user: {
          id: 'mock-google-user-id',
          email: 'mock@google.com',
          fullName: 'Mock Google User',
          role: 'user'
        }
      };
    }
  }
};

// Enhanced mock data for Houston area individuals
const mockHoustonData = {
    individuals: [
        {
            id: 1,
            fullName: "Sarah Johnson",
            currentStatus: "Missing",
            dateOfBirth: "1995-03-15",
            gender: "Female",
            address: "123 Main St",
            city: "Houston",
            state: "TX",
            latitude: 29.7604,
            longitude: -95.3698,
            dateAdded: "2025-01-15T10:30:00Z",
            lastSeen: "2025-01-15T08:00:00Z",
            description: "Brown hair, blue eyes, wearing red jacket",
            contactInfo: "713-555-0101"
        },
        {
            id: 2,
            fullName: "Michael Chen",
            currentStatus: "Found",
            dateOfBirth: "1988-07-22",
            gender: "Male",
            address: "456 Oak Ave",
            city: "Spring",
            state: "TX",
            latitude: 30.0799,
            longitude: -95.4172,
            dateAdded: "2025-01-10T14:20:00Z",
            lastSeen: "2025-01-10T12:00:00Z",
            description: "Asian male, black hair, wearing business attire",
            contactInfo: "281-555-0202"
        },
        {
            id: 3,
            fullName: "Emily Rodriguez",
            currentStatus: "Safe",
            dateOfBirth: "1992-11-08",
            gender: "Female",
            address: "789 Pine Rd",
            city: "Katy",
            state: "TX",
            latitude: 29.7858,
            longitude: -95.8244,
            dateAdded: "2025-01-08T09:15:00Z",
            lastSeen: "2025-01-08T07:30:00Z",
            description: "Hispanic female, long brown hair, wearing running clothes",
            contactInfo: "281-555-0303"
        },
        {
            id: 4,
            fullName: "David Thompson",
            currentStatus: "Missing",
            dateOfBirth: "1990-05-12",
            gender: "Male",
            address: "321 Elm St",
            city: "Sugar Land",
            state: "TX",
            latitude: 29.6197,
            longitude: -95.6349,
            dateAdded: "2025-01-12T16:45:00Z",
            lastSeen: "2025-01-12T15:00:00Z",
            description: "Caucasian male, brown hair, wearing jeans and t-shirt",
            contactInfo: "281-555-0404"
        },
        {
            id: 5,
            fullName: "Lisa Wang",
            currentStatus: "Found",
            dateOfBirth: "1987-09-30",
            gender: "Female",
            address: "654 Maple Dr",
            city: "The Woodlands",
            state: "TX",
            latitude: 30.1658,
            longitude: -95.4612,
            dateAdded: "2025-01-05T11:30:00Z",
            lastSeen: "2025-01-05T10:00:00Z",
            description: "Asian female, black hair, wearing casual clothes",
            contactInfo: "281-555-0505"
        },
        {
            id: 6,
            fullName: "James Wilson",
            currentStatus: "Urgent",
            dateOfBirth: "1993-12-03",
            gender: "Male",
            address: "987 Cedar Ln",
            city: "Pearland",
            state: "TX",
            latitude: 29.5636,
            longitude: -95.2864,
            dateAdded: "2025-01-14T13:20:00Z",
            lastSeen: "2025-01-14T12:00:00Z",
            description: "African American male, short hair, wearing athletic wear",
            contactInfo: "281-555-0606"
        }
    ]
};

// Enhanced Houston statistics
const mockHoustonStats = {
    totalIndividuals: 12,
    missingCases: 4,
    foundCases: 3,
    safeCases: 4,
    urgentCases: 2,
    recentCases: 8,
    statusBreakdown: [
        { status: "Missing", count: 4 },
        { status: "Found", count: 3 },
        { status: "Safe", count: 4 },
        { status: "Urgent", count: 2 }
    ],
    recentActivity: [
        { type: "New Case", description: "Jennifer Lee reported missing", time: "2 hours ago" },
        { type: "Case Update", description: "Michael Chen found safe", time: "1 day ago" },
        { type: "New Case", description: "James Wilson reported missing", time: "1 day ago" },
        { type: "Case Update", description: "Sarah Johnson status updated", time: "2 days ago" }
    ]
};

// Enhanced geocoding function with more neighborhoods
function mockGeocode(address) {
    const houstonNeighborhoods = {
        "Downtown Houston": { lat: 29.7604, lng: -95.3698 },
        "Midtown Houston": { lat: 29.7458, lng: -95.3656 },
        "Montrose": { lat: 29.7420, lng: -95.3796 },
        "Heights": { lat: 29.8020, lng: -95.4012 },
        "Rice Village": { lat: 29.7205, lng: -95.4156 },
        "Museum District": { lat: 29.7230, lng: -95.3856 },
        "Medical Center": { lat: 29.7070, lng: -95.3970 },
        "Galleria": { lat: 29.7400, lng: -95.4600 },
        "West University": { lat: 29.7200, lng: -95.4300 },
        "Bellaire": { lat: 29.7050, lng: -95.4600 },
        "River Oaks": { lat: 29.7500, lng: -95.4200 },
        "Memorial": { lat: 29.7800, lng: -95.4500 },
        "Spring Branch": { lat: 29.8000, lng: -95.5000 },
        "Garden Oaks": { lat: 29.8200, lng: -95.4200 },
        "Oak Forest": { lat: 29.8300, lng: -95.4300 },
        "Timber Grove": { lat: 29.8400, lng: -95.4400 },
        "Cypress": { lat: 29.9691, lng: -95.6972 },
        "Spring": { lat: 30.0799, lng: -95.4172 },
        "The Woodlands": { lat: 30.1658, lng: -95.4612 },
        "Katy": { lat: 29.7858, lng: -95.8244 },
        "Sugar Land": { lat: 29.6197, lng: -95.6349 },
        "Pearland": { lat: 29.5636, lng: -95.2860 },
        "League City": { lat: 29.5074, lng: -95.0949 },
        "Friendswood": { lat: 29.5294, lng: -95.2010 },
        "Missouri City": { lat: 29.6185, lng: -95.5377 },
        "Rosenberg": { lat: 29.5572, lng: -95.8085 },
        "Tomball": { lat: 30.0952, lng: -95.6160 }
    };

    // Try to find exact match
    for (const [neighborhood, coords] of Object.entries(houstonNeighborhoods)) {
        if (address.toLowerCase().includes(neighborhood.toLowerCase())) {
            return coords;
        }
    }

    // Return downtown Houston as default
    return { lat: 29.7604, lng: -95.3698 };
}

// Real-time data simulation
function simulateRealtimeData() {
    const newCase = {
        id: Math.floor(Math.random() * 10000) + 13,
        fullName: `Case ${Math.floor(Math.random() * 1000)}`,
        currentStatus: ["Missing", "Urgent", "Safe"][Math.floor(Math.random() * 3)],
        dateOfBirth: "1990-01-01",
        gender: ["Male", "Female"][Math.floor(Math.random() * 2)],
        address: "Random Address",
        city: "Houston",
        state: "TX",
        latitude: 29.7604 + (Math.random() - 0.5) * 0.1,
        longitude: -95.3698 + (Math.random() - 0.5) * 0.1,
        dateAdded: new Date().toISOString(),
        lastSeen: new Date(Date.now() - Math.random() * 86400000).toISOString(),
        description: "Auto-generated case",
        contactInfo: "281-555-0000"
    };
    
    return newCase;
}

// Mock API server for testing
if (typeof window !== 'undefined') {
  // Browser environment - override fetch for testing
  const originalFetch = window.fetch;
  window.fetch = async function(url, options = {}) {
    const urlObj = new URL(url);
    const path = urlObj.pathname;
    
    if (mockApi[path] && mockApi[path].method === options.method) {
      console.log(`Mock API called: ${options.method} ${path}`);
      
      let requestData = null;
      if (options.body) {
        try {
          requestData = JSON.parse(options.body);
        } catch (e) {
          console.error('Failed to parse request body:', e);
        }
      }
      
      const response = mockApi[path].response(requestData);
      
      return new Response(JSON.stringify(response), {
        status: response.success ? 200 : 400,
        headers: {
          'Content-Type': 'application/json'
        }
      });
    }
    
    // Fall back to original fetch for non-mock endpoints
    return originalFetch(url, options);
  };
}

// Export for use in HTML
window.mockHoustonData = mockHoustonData;
window.mockHoustonStats = mockHoustonStats;
window.mockGeocode = mockGeocode;
window.simulateRealtimeData = simulateRealtimeData; 