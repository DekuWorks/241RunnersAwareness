/**
 * ============================================
 * GOOGLE MAPS CONFIGURATION
 * ============================================
 * 
 * Configuration for Google Maps API integration
 * Replace YOUR_GOOGLE_MAPS_API_KEY with your actual API key
 */

window.GOOGLE_MAPS_CONFIG = {
    // Your actual Google Maps API key
    API_KEY: 'AIzaSyC54u396zN1n1tNMcqaf6CySrku-Pu50Fw',
    
    // Default map center (Houston, TX)
    DEFAULT_CENTER: {
        lat: 29.7604,
        lng: -95.3698
    },
    
    // Default zoom level
    DEFAULT_ZOOM: 10,
    
    // Map styles for better appearance
    MAP_STYLES: [
        {
            "featureType": "all",
            "elementType": "geometry.fill",
            "stylers": [
                {
                    "weight": "2.00"
                }
            ]
        },
        {
            "featureType": "all",
            "elementType": "geometry.stroke",
            "stylers": [
                {
                    "color": "#9c9c9c"
                }
            ]
        },
        {
            "featureType": "all",
            "elementType": "labels.text",
            "stylers": [
                {
                    "visibility": "on"
                }
            ]
        },
        {
            "featureType": "landscape",
            "elementType": "all",
            "stylers": [
                {
                    "color": "#f2f2f2"
                }
            ]
        },
        {
            "featureType": "landscape",
            "elementType": "geometry.fill",
            "stylers": [
                {
                    "color": "#ffffff"
                }
            ]
        },
        {
            "featureType": "landscape.man_made",
            "elementType": "geometry.fill",
            "stylers": [
                {
                    "color": "#ffffff"
                }
            ]
        },
        {
            "featureType": "poi",
            "elementType": "all",
            "stylers": [
                {
                    "visibility": "off"
                }
            ]
        },
        {
            "featureType": "road",
            "elementType": "all",
            "stylers": [
                {
                    "saturation": -100
                },
                {
                    "lightness": 45
                }
            ]
        },
        {
            "featureType": "road",
            "elementType": "geometry.fill",
            "stylers": [
                {
                    "color": "#eeeeee"
                }
            ]
        },
        {
            "featureType": "road",
            "elementType": "labels.text.fill",
            "stylers": [
                {
                    "color": "#7b7b7b"
                }
            ]
        },
        {
            "featureType": "road",
            "elementType": "labels.text.stroke",
            "stylers": [
                {
                    "color": "#ffffff"
                }
            ]
        },
        {
            "featureType": "road.highway",
            "elementType": "all",
            "stylers": [
                {
                    "visibility": "simplified"
                }
            ]
        },
        {
            "featureType": "road.arterial",
            "elementType": "labels.icon",
            "stylers": [
                {
                    "visibility": "off"
                }
            ]
        },
        {
            "featureType": "transit",
            "elementType": "all",
            "stylers": [
                {
                    "visibility": "off"
                }
            ]
        },
        {
            "featureType": "water",
            "elementType": "all",
            "stylers": [
                {
                    "color": "#46bcec"
                },
                {
                    "visibility": "on"
                }
            ]
        },
        {
            "featureType": "water",
            "elementType": "geometry.fill",
            "stylers": [
                {
                    "color": "#c8d7d4"
                }
            ]
        },
        {
            "featureType": "water",
            "elementType": "labels.text.fill",
            "stylers": [
                {
                    "color": "#070707"
                }
            ]
        },
        {
            "featureType": "water",
            "elementType": "labels.text.stroke",
            "stylers": [
                {
                    "color": "#ffffff"
                }
            ]
        }
    ],
    
    // Marker clusterer configuration
    CLUSTER_CONFIG: {
        imagePath: "https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m",
        maxZoom: 15,
        gridSize: 60,
        styles: [
            {
                url: "https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m1.png",
                width: 56,
                height: 56,
                textColor: "#ffffff",
                textSize: 12
            },
            {
                url: "https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m2.png",
                width: 56,
                height: 56,
                textColor: "#ffffff",
                textSize: 12
            },
            {
                url: "https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m3.png",
                width: 56,
                height: 56,
                textColor: "#ffffff",
                textSize: 12
            },
            {
                url: "https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m4.png",
                width: 56,
                height: 56,
                textColor: "#ffffff",
                textSize: 12
            },
            {
                url: "https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m5.png",
                width: 56,
                height: 56,
                textColor: "#ffffff",
                textSize: 12
            }
        ]
    },
    
    // Heat map configuration
    HEATMAP_CONFIG: {
        radius: 20,
        opacity: 0.6
    },
    
    // Status colors for markers
    STATUS_COLORS: {
        missing: '#f59e0b',
        found: '#dc2626',
        safe: '#10b981',
        urgent: '#ef4444',
        deceased: '#6b7280'
    }
};

// Log configuration load
console.log('🗺️ Google Maps configuration loaded');
console.log('🔑 API Key configured:', window.GOOGLE_MAPS_CONFIG.API_KEY !== 'YOUR_GOOGLE_MAPS_API_KEY');
