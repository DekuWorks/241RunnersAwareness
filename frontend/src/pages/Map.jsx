import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { MapContainer, TileLayer, CircleMarker, Popup } from 'react-leaflet';
import Cluster from 'react-leaflet-cluster';
import 'leaflet/dist/leaflet.css';
import 'leaflet.markercluster/dist/MarkerCluster.css';
import 'leaflet.markercluster/dist/MarkerCluster.Default.css';

const Map = () => {
    const navigate = useNavigate();
    const [runners, setRunners] = useState([]);
    const [filteredRunners, setFilteredRunners] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [statusFilter, setStatusFilter] = useState('');
    const [clustersEnabled, setClustersEnabled] = useState(true);

    // Load runners data
    useEffect(() => {
        loadRunnersData();
    }, []);

    // Filter runners when status filter changes
    useEffect(() => {
        if (statusFilter) {
            setFilteredRunners(runners.filter(runner => runner.currentStatus === statusFilter));
        } else {
            setFilteredRunners(runners);
        }
    }, [runners, statusFilter]);

    const loadRunnersData = async () => {
        setLoading(true);
        setError(null);

        try {
            const response = await fetch('/api/individuals');
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const data = await response.json();
            const runnersWithLocation = data.filter(runner => 
                runner.latitude && runner.longitude && 
                runner.latitude !== 0 && runner.longitude !== 0
            );
            
            setRunners(runnersWithLocation);
            setLoading(false);
            
        } catch (error) {
            console.error('Error loading runners data:', error);
            setError(`Error loading data: ${error.message}`);
            setLoading(false);
        }
    };

    const getMarkerColor = (status) => {
        const colors = {
            'Found': '#ff6b6b',
            'Missing': '#ffa500', 
            'Safe': '#51cf66'
        };
        return colors[status] || '#666';
    };

    const calculateAge = (dateOfBirth) => {
        if (!dateOfBirth) return 'Unknown';
        const birthDate = new Date(dateOfBirth);
        const today = new Date();
        let age = today.getFullYear() - birthDate.getFullYear();
        const monthDiff = today.getMonth() - birthDate.getMonth();
        if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
            age--;
        }
        return age;
    };

    const createPopupContent = (runner) => {
        const statusClass = runner.currentStatus?.toLowerCase().replace(/\s+/g, '-') || 'unknown';
        return (
            <div className="runner-popup">
                <h3>{runner.fullName}</h3>
                <p><strong>Status:</strong> <span className={`status-badge status-${statusClass}`}>{runner.currentStatus || 'Unknown'}</span></p>
                <p><strong>Age:</strong> {calculateAge(runner.dateOfBirth)} years old</p>
                <p><strong>Gender:</strong> {runner.gender || 'Not specified'}</p>
                {runner.address && <p><strong>Address:</strong> {runner.address}</p>}
                {runner.city && runner.state && <p><strong>Location:</strong> {runner.city}, {runner.state}</p>}
                <p><strong>Added:</strong> {new Date(runner.dateAdded).toLocaleDateString()}</p>
            </div>
        );
    };

    if (loading) {
        return (
            <div className="loading">
                Loading map data...
            </div>
        );
    }

    if (error) {
        return (
            <div className="error">
                {error}
            </div>
        );
    }

    if (runners.length === 0) {
        return (
            <div className="no-data">
                No location data available. Runners need to have latitude and longitude coordinates to appear on the map.
            </div>
        );
    }

    return (
        <div className="map-page">
            <div className="back-home-container">
                <button onClick={() => navigate('/')} className="back-home-btn">
                    ← Back to Home
                </button>
            </div>

            <h1>Map View</h1>
            <p>View the locations of all registered runners on an interactive map.</p>

            <div className="map-controls">
                <button onClick={loadRunnersData} className="control-btn">
                    🔄 Refresh Data
                </button>
                <select 
                    value={statusFilter} 
                    onChange={(e) => setStatusFilter(e.target.value)}
                    className="control-select"
                >
                    <option value="">All Statuses</option>
                    <option value="Found">Found</option>
                    <option value="Missing">Missing</option>
                    <option value="Safe">Safe</option>
                </select>
                <button onClick={() => setClustersEnabled(!clustersEnabled)} className="control-btn">
                    🗂️ {clustersEnabled ? 'Disable' : 'Enable'} Clusters
                </button>
            </div>

            <div className="map-container">
                <MapContainer 
                    center={[39.8283, -98.5795]} 
                    zoom={4} 
                    style={{ height: '70vh', width: '100%' }}
                >
                    <TileLayer
                        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                        attribution='© OpenStreetMap contributors'
                    />
                    
                    {clustersEnabled ? (
                        <Cluster>
                            {filteredRunners.map((runner, index) => (
                                <CircleMarker
                                    key={runner.individualId || index}
                                    center={[runner.latitude, runner.longitude]}
                                    radius={8}
                                    fillColor={getMarkerColor(runner.currentStatus)}
                                    color="#fff"
                                    weight={2}
                                    opacity={1}
                                    fillOpacity={0.8}
                                >
                                    <Popup>{createPopupContent(runner)}</Popup>
                                </CircleMarker>
                            ))}
                        </Cluster>
                    ) : (
                        filteredRunners.map((runner, index) => (
                            <CircleMarker
                                key={runner.individualId || index}
                                center={[runner.latitude, runner.longitude]}
                                radius={8}
                                fillColor={getMarkerColor(runner.currentStatus)}
                                color="#fff"
                                weight={2}
                                opacity={1}
                                fillOpacity={0.8}
                            >
                                <Popup>{createPopupContent(runner)}</Popup>
                            </CircleMarker>
                        ))
                    )}
                </MapContainer>
            </div>

            <div className="map-info">
                <h3>Map Legend</h3>
                <div className="legend">
                    <div className="legend-item">
                        <span className="status-badge status-found">Found</span>
                        <span>Runner has been found</span>
                    </div>
                    <div className="legend-item">
                        <span className="status-badge status-missing">Missing</span>
                        <span>Runner is currently missing</span>
                    </div>
                    <div className="legend-item">
                        <span className="status-badge status-safe">Safe</span>
                        <span>Runner is safe</span>
                    </div>
                </div>
            </div>

            <style jsx>{`
                .map-page {
                    padding: 20px;
                    max-width: 1200px;
                    margin: 0 auto;
                }

                .map-container {
                    border-radius: 8px;
                    margin: 20px 0;
                    overflow: hidden;
                }

                .map-controls {
                    display: flex;
                    gap: 10px;
                    margin-bottom: 20px;
                    flex-wrap: wrap;
                }

                .control-btn {
                    padding: 8px 16px;
                    border: none;
                    border-radius: 4px;
                    background: var(--accent-color);
                    color: white;
                    cursor: pointer;
                    transition: background-color 0.3s;
                }

                .control-btn:hover {
                    background: var(--accent-hover);
                }

                .control-select {
                    padding: 8px;
                    border: 1px solid var(--border-color);
                    border-radius: 4px;
                    background: var(--bg-color);
                    color: var(--text-color);
                }

                .runner-popup {
                    text-align: center;
                }

                .runner-popup h3 {
                    margin: 0 0 10px 0;
                    color: var(--accent-color);
                }

                .runner-popup p {
                    margin: 5px 0;
                    font-size: 14px;
                }

                .status-badge {
                    display: inline-block;
                    padding: 2px 8px;
                    border-radius: 12px;
                    font-size: 12px;
                    font-weight: bold;
                    text-transform: uppercase;
                }

                .status-found { background: #ff6b6b; color: white; }
                .status-missing { background: #ffa500; color: white; }
                .status-safe { background: #51cf66; color: white; }

                .loading {
                    text-align: center;
                    padding: 40px;
                    color: var(--text-muted);
                }

                .error {
                    background: #ff6b6b;
                    color: white;
                    padding: 15px;
                    border-radius: 8px;
                    margin: 20px 0;
                }

                .no-data {
                    text-align: center;
                    padding: 40px;
                    color: var(--text-muted);
                    font-style: italic;
                }

                .legend {
                    display: flex;
                    gap: 20px;
                    flex-wrap: wrap;
                    margin-top: 10px;
                }

                .legend-item {
                    display: flex;
                    align-items: center;
                    gap: 8px;
                }
            `}</style>
        </div>
    );
};

export default Map; 