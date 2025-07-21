// Mock API for Houston Map Data
// This provides sample data for the Houston area map

// Houston coordinates
const HOUSTON_LAT = 29.7604;
const HOUSTON_LNG = -95.3698;

// Mock data for Houston area individuals
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
            dateAdded: "2025-01-15T10:30:00Z"
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
            dateAdded: "2025-01-10T14:20:00Z"
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
            dateAdded: "2025-01-08T09:15:00Z"
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
            dateAdded: "2025-01-12T16:45:00Z"
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
            dateAdded: "2025-01-05T11:30:00Z"
        }
    ]
};

// Mock Houston statistics
const mockHoustonStats = {
    totalIndividuals: 5,
    missingCases: 2,
    foundCases: 2,
    safeCases: 1,
    recentCases: 3,
    statusBreakdown: [
        { status: "Missing", count: 2 },
        { status: "Found", count: 2 },
        { status: "Safe", count: 1 }
    ]
};

// Mock geocoding function
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
        "Bellaire": { lat: 29.7050, lng: -95.4600 }
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

// Export for use in HTML
window.mockHoustonData = mockHoustonData;
window.mockHoustonStats = mockHoustonStats;
window.mockGeocode = mockGeocode; 